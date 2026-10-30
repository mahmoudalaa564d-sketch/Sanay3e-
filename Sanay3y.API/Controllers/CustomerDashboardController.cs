
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerDashboardController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CustomerDashboard/summary
        // Get customer dashboard summary.
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            CancellationToken cancellationToken)
        {
            var idValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(idValue, out int customerId))
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the customer account."
                });
            }

            var customerIsActive = await _context.Users
                .AsNoTracking()
                .AnyAsync(
                    u => u.Id == customerId &&
                         u.Role == "Customer" &&
                         u.AccountStatus == "Active",
                    cancellationToken);

            if (!customerIsActive)
            {
                return Forbid();
            }

            // Orders statistics
            var totalOrders = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(
                    r => r.CustomerUserId == customerId,
                    cancellationToken);

            var activeOrders = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(
                    r => r.CustomerUserId == customerId &&
                         (
                             r.Status == "PendingOffers" ||
                             r.Status == "OfferAccepted" ||
                             r.Status == "InProgress" ||
                             r.Status == "AwaitingPayment"
                         ),
                    cancellationToken);

            var completedOrders = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(
                    r => r.CustomerUserId == customerId &&
                         r.Status == "Completed",
                    cancellationToken);

            var cancelledOrders = await _context.ServiceRequests
                .AsNoTracking()
                .CountAsync(
                    r => r.CustomerUserId == customerId &&
                         r.Status == "Cancelled",
                    cancellationToken);

            // Wallet information
            var wallet = await _context.Wallets
                .AsNoTracking()
                .Where(w => w.UserId == customerId)
                .Select(w => new
                {
                    w.Id,
                    w.Balance,
                    w.Currency
                })
                .FirstOrDefaultAsync(cancellationToken);

            decimal totalDeposits = 0m;
            decimal totalWithdrawals = 0m;

            var recentTransactions = new object[0];

            if (wallet != null)
            {
                // Total completed deposits
                totalDeposits = await _context.WalletTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.WalletId == wallet.Id &&
                        t.Type == "TopUp" &&
                        t.Status == "Completed")
                    .SumAsync(
                        t => (decimal?)t.Amount,
                        cancellationToken) ?? 0m;

                // Total completed withdrawals
                totalWithdrawals = await _context.WalletTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.WalletId == wallet.Id &&
                        t.Type == "WithdrawalDebit" &&
                        t.Status == "Completed")
                    .SumAsync(
                        t => (decimal?)t.Amount,
                        cancellationToken) ?? 0m;

                // Latest 5 wallet transactions
                recentTransactions = await _context.WalletTransactions
                    .AsNoTracking()
                    .Where(t => t.WalletId == wallet.Id)
                    .OrderByDescending(t => t.CreatedAtUtc)
                    .ThenByDescending(t => t.Id)
                    .Take(5)
                    .Select(t => new
                    {
                        id = t.Id,
                        type = t.Type,
                        direction = t.Direction,
                        amount = t.Amount,
                        balanceAfter = t.BalanceAfter,
                        status = t.Status,
                        description = t.Description,
                        createdAtUtc = t.CreatedAtUtc
                    })
                    .Cast<object>()
                    .ToArrayAsync(cancellationToken);
            }

            // Total payments for service requests
            var totalSpent = await _context.ServicePayments
                .AsNoTracking()
                .Where(p =>
                    p.CustomerUserId == customerId &&
                    p.Status == "Paid" &&
                    p.Currency == "EGP")
                .SumAsync(
                    p => (decimal?)p.Amount,
                    cancellationToken);

            return Ok(new
            {
                customerId,

                orders = new
                {
                    total = totalOrders,
                    active = activeOrders,
                    completed = completedOrders,
                    cancelled = cancelledOrders
                },

                wallet = new
                {
                    balance = wallet?.Balance ?? 0m,
                    currency = wallet?.Currency ?? "EGP",
                    totalDeposits,
                    totalWithdrawals,
                    recentTransactions
                },

                payments = new
                {
                    totalSpent = totalSpent ?? 0m,
                    currency = "EGP"
                }
            });
        }

        // GET: api/CustomerDashboard/specialties
        // Get all active platform specialties.
        [HttpGet("specialties")]
        public async Task<IActionResult> GetSpecialties(
            CancellationToken cancellationToken)
        {
            var specialties = await _context.Specialties
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    nameEn = s.NameEn,
                    description = s.Description,
                    sortOrder = s.SortOrder
                })
                .ToListAsync(cancellationToken);

            return Ok(specialties);
        }

        // GET: api/CustomerDashboard/recommended-artisans
        // Get approved and active artisans with their ratings.
        [HttpGet("recommended-artisans")]
        public async Task<IActionResult> GetRecommendedArtisans(
            CancellationToken cancellationToken)
        {
            var artisans = await _context.ArtisanProfiles
                .AsNoTracking()
                .Where(a =>
                    a.ApprovalStatus == "Approved" &&
                    a.User.Role == "Artisan" &&
                    a.User.AccountStatus == "Active")
                .Select(a => new
                {
                    artisanId = a.UserId,
                    fullName = a.User.FullName,
                    profession = a.Profession,
                    experienceYears = a.ExperienceYears,
                    bio = a.Bio,
                    profileImagePath = a.ProfileImagePath,

                    specialties = a.Specialties
                        .Select(s => s.Name)
                        .ToList(),

                    averageRating = _context.ServiceReviews
                        .Where(r => r.ArtisanUserId == a.UserId)
                        .Select(r => (double?)r.Rating)
                        .Average(),

                    totalReviews = _context.ServiceReviews
                        .Count(r => r.ArtisanUserId == a.UserId)
                })
                .OrderByDescending(a => a.averageRating)
                .ThenByDescending(a => a.totalReviews)
                .ToListAsync(cancellationToken);

            var result = artisans.Select(a => new
            {
                a.artisanId,
                a.fullName,
                a.profession,
                a.experienceYears,
                a.bio,
                a.profileImagePath,
                a.specialties,
                averageRating = a.averageRating ?? 0,
                a.totalReviews
            });

            return Ok(result);
        }

        // GET: api/CustomerDashboard/search-services?query=plumber
        // Search active specialties and services provided by approved artisans.
        [HttpGet("search-services")]
        public async Task<IActionResult> SearchServices(
            [FromQuery] string? query,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new
                {
                    message = "Please enter a search term."
                });
            }

            query = query.Trim();

            var specialties = await _context.Specialties
                .AsNoTracking()
                .Where(s =>
                    s.IsActive &&
                    (
                        s.Name.Contains(query) ||
                        s.NameEn.Contains(query)
                    ))
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Id)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    nameEn = s.NameEn,
                    description = s.Description,
                    type = "Specialty"
                })
                .ToListAsync(cancellationToken);

            var services = await _context.ArtisanServiceTypes
                .AsNoTracking()
                .Where(s =>
                    s.Name.Contains(query) &&
                    s.ArtisanProfile.ApprovalStatus == "Approved" &&
                    s.ArtisanProfile.User.Role == "Artisan" &&
                    s.ArtisanProfile.User.AccountStatus == "Active")
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    artisanId = s.ArtisanUserId,
                    artisanName = s.ArtisanProfile.User.FullName,
                    profession = s.ArtisanProfile.Profession,
                    type = "Service"
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                query,
                totalResults = specialties.Count + services.Count,
                specialties,
                services
            });
        }

        // GET: api/CustomerDashboard/profile
        // Get the currently authenticated customer's profile.
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(
            CancellationToken cancellationToken)
        {
            var idValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(idValue, out int customerId))
            {
                return Unauthorized(new
                {
                    message = "Unable to identify the customer account."
                });
            }

            var customer = await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.Id == customerId &&
                    u.Role == "Customer")
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber,
                    role = u.Role,
                    accountStatus = u.AccountStatus,
                    isEmailVerified = u.IsEmailVerified,
                    isPhoneVerified = u.IsPhoneVerified,
                    createdAtUtc = u.CreatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer account was not found."
                });
            }

            if (customer.accountStatus != "Active")
            {
                return Forbid();
            }

            return Ok(customer);
        }
    }
}
