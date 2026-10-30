using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerOffersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerOffersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CustomerOffers/requests/1/offers
        // Customer views offers for their own request.
        [HttpGet("requests/{requestId:int}/offers")]
        public async Task<IActionResult> GetOffers(
            int requestId,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerId(out int customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            var serviceRequest = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == requestId &&
                         r.CustomerUserId == customerId,
                    cancellationToken);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message = "طلب الخدمة غير موجود."
                });
            }

            var offers = await _context.ServiceOffers
                .AsNoTracking()
                .Where(o => o.ServiceRequestId == requestId)
                .Include(o => o.Artisan)
                    .ThenInclude(a => a.ArtisanProfile)
                .OrderBy(o => o.Price)
                .ThenByDescending(o => o.CreatedAtUtc)
                .Select(o => new
                {
                    o.Id,
                    o.ServiceRequestId,
                    o.Price,
                    o.Message,
                    o.EstimatedArrivalMinutes,
                    o.Status,
                    o.CreatedAtUtc,

                    Artisan = new
                    {
                        UserId = o.ArtisanUserId,
                        FullName = o.Artisan.FullName,

                        Profession = o.Artisan.ArtisanProfile != null
                            ? o.Artisan.ArtisanProfile.Profession
                            : null,

                        ExperienceYears = o.Artisan.ArtisanProfile != null
                            ? o.Artisan.ArtisanProfile.ExperienceYears
                            : (int?)null,

                        ServiceArea = o.Artisan.ArtisanProfile != null
                            ? o.Artisan.ArtisanProfile.ServiceArea
                            : null,

                        ProfileImagePath = o.Artisan.ArtisanProfile != null
                            ? o.Artisan.ArtisanProfile.ProfileImagePath
                            : null
                    }
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                requestId = serviceRequest.Id,
                requestNumber = serviceRequest.RequestNumber,
                requestStatus = serviceRequest.Status,
                totalOffers = offers.Count,
                offers
            });
        }

        // POST:
        // api/CustomerOffers/requests/1/offers/5/accept
        // Accept one offer and reject the other pending offers.
        [HttpPost("requests/{requestId:int}/offers/{offerId:int}/accept")]
        public async Task<IActionResult> AcceptOffer(
            int requestId,
            int offerId,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerId(out int customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            // Serialize competing attempts to accept offers.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.Id == requestId &&
                         r.CustomerUserId == customerId,
                    cancellationToken);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message = "طلب الخدمة غير موجود."
                });
            }

            if (serviceRequest.Status != "PendingOffers")
            {
                return Conflict(new
                {
                    message = "الطلب لا يستقبل اختيارات عروض جديدة حاليًا."
                });
            }

            var selectedOffer = await _context.ServiceOffers
                .FirstOrDefaultAsync(
                    o => o.Id == offerId &&
                         o.ServiceRequestId == requestId &&
                         o.Status == "Pending",
                    cancellationToken);

            if (selectedOffer == null)
            {
                return NotFound(new
                {
                    message = "العرض غير موجود أو لم يعد متاحًا للاختيار."
                });
            }

            // Get artisan name.
            var artisanName = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == selectedOffer.ArtisanUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(artisanName))
            {
                artisanName = "الصنايعي";
            }

            // Get all pending offers for this request.
            var pendingOffers = await _context.ServiceOffers
                .Where(o => o.ServiceRequestId == requestId &&
                            o.Status == "Pending")
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            // Accept selected offer and reject the others.
            foreach (var offer in pendingOffers)
            {
                offer.Status = offer.Id == selectedOffer.Id
                    ? "Accepted"
                    : "Rejected";

                offer.UpdatedAtUtc = now;
            }

            // Update service request status.
            serviceRequest.Status = "OfferAccepted";
            serviceRequest.UpdatedAtUtc = now;

            // Create notification for the customer.
            var notification = new Notification
            {
                UserId = serviceRequest.CustomerUserId,
                Title = "تم قبول عرض الصنايعي",
                Message = $"تم قبول عرض {artisanName} لطلبك رقم {serviceRequest.RequestNumber}.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = now,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = serviceRequest.Id
            };

            _context.Notifications.Add(notification);

            try
            {
                // Save all changes in the same transaction.
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "تعذر قبول العرض. ربما تم اختيار عرض آخر لهذا الطلب بالفعل."
                });
            }

            return Ok(new
            {
                message = "تم قبول العرض بنجاح.",

                requestId = serviceRequest.Id,
                requestNumber = serviceRequest.RequestNumber,
                requestStatus = serviceRequest.Status,

                acceptedOffer = new
                {
                    selectedOffer.Id,
                    selectedOffer.ArtisanUserId,
                    selectedOffer.Price,
                    selectedOffer.Message,
                    selectedOffer.EstimatedArrivalMinutes,
                    selectedOffer.Status
                },

                notification = new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.IsRead,
                    notification.CreatedAtUtc
                }
            });
        }

        // Get customer ID from JWT token.
        private bool TryGetCustomerId(out int customerId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out customerId);
        }
    }
}