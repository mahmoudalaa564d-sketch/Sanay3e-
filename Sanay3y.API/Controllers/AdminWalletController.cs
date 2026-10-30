
using System;
using System.Data;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminWalletController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminWalletController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: api/AdminWallet/users/2/test-credit
        // Development only: add test balance to a user wallet
        [HttpPost("users/{userId:int}/test-credit")]
        public async Task<IActionResult> AddTestCredit(
            int userId,
            [FromBody] AdminTestWalletCreditDto input,
            CancellationToken cancellationToken)
        {
            // This endpoint must never operate in production
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            if (input.Amount < 1m ||
                input.Amount > 10000m ||
                decimal.Round(input.Amount, 2) != input.Amount)
            {
                return BadRequest(new
                {
                    message = "المبلغ يجب أن يكون بين 1 و10000 جنيه وبحد أقصى منزلتين عشريتين."
                });
            }

            if (!TryGetUserId(out int adminUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب المدير."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Only active customers and artisans can receive test credit
            var targetUser = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == userId &&
                         (u.Role == "Customer" ||
                          u.Role == "Artisan") &&
                         u.AccountStatus == "Active",
                    cancellationToken);

            if (targetUser == null)
            {
                return NotFound(new
                {
                    message = "الحساب غير موجود أو غير نشط أو ليس عميلًا أو صنايعيًا."
                });
            }

            // Find or create the target wallet
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(
                    w => w.UserId == userId,
                    cancellationToken);

            var now = DateTime.UtcNow;

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0m,
                    Currency = "EGP",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                };

                _context.Wallets.Add(wallet);

                await _context.SaveChangesAsync(
                    cancellationToken);
            }

            // Create a permanent test top-up record
            var topUp = new WalletTopUp
            {
                WalletId = wallet.Id,
                Amount = input.Amount,
                Currency = "EGP",
                Provider = "DevelopmentTest",
                SpecialReference =
                    $"SANAY3Y-DEV-{Guid.NewGuid():N}",
                Status = "Paid",
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                PaidAtUtc = now
            };

            // Add the test balance
            wallet.Balance += input.Amount;
            wallet.UpdatedAtUtc = now;

            _context.WalletTopUps.Add(topUp);

            // Record the credit in the wallet ledger
            var walletTransaction = new WalletTransaction
            {
                Wallet = wallet,
                Type = "TopUp",
                Direction = "Credit",
                Amount = input.Amount,
                BalanceAfter = wallet.Balance,
                Status = "Completed",
                Description =
                    $"رصيد اختباري أضافه المدير رقم {adminUserId}",
                Provider = "DevelopmentTest",
                CreatedAtUtc = now
            };

            _context.WalletTransactions.Add(walletTransaction);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "تم إضافة الرصيد الاختباري بنجاح.",
                walletId = wallet.Id,
                userId = wallet.UserId,
                userRole = targetUser.Role,
                addedAmount = input.Amount,
                currency = wallet.Currency,
                currentBalance = wallet.Balance,
                topUpId = topUp.Id,
                transactionId = walletTransaction.Id,
                status = "Completed",
                note = "Development test credit only. No real money was received."
            });
        }

        private bool TryGetUserId(out int userId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out userId);
        }
    }
}
