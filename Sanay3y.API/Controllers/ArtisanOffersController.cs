
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Artisan")]
    public class ArtisanOffersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArtisanOffersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/ArtisanOffers/requests/1/offers
        // Artisan sends an offer and the customer receives a notification.
        [HttpPost("requests/{requestId:int}/offers")]
        public async Task<IActionResult> CreateOffer(
            int requestId,
            [FromBody] CreateServiceOfferDto input,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int artisanUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب الصنايعي من رمز الدخول."
                });
            }

            var artisan = await _context.Users
                .Include(u => u.ArtisanProfile)
                .FirstOrDefaultAsync(
                    u => u.Id == artisanUserId,
                    cancellationToken);

            if (artisan == null || artisan.Role != "Artisan")
            {
                return Forbid();
            }

            if (artisan.AccountStatus != "Active" ||
                artisan.ArtisanProfile == null ||
                artisan.ArtisanProfile.ApprovalStatus != "Approved")
            {
                return StatusCode(403, new
                {
                    message = "يجب تفعيل حساب الصنايعي واعتماده من الإدارة قبل إرسال العروض."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.Id == requestId,
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
                    message = "الطلب لا يستقبل عروضًا جديدة حاليًا."
                });
            }

            var existingOffer = await _context.ServiceOffers
                .AnyAsync(
                    o => o.ServiceRequestId == requestId &&
                         o.ArtisanUserId == artisanUserId,
                    cancellationToken);

            if (existingOffer)
            {
                return Conflict(new
                {
                    message = "سبق لك إرسال عرض على الطلب ده."
                });
            }

            var now = DateTime.UtcNow;

            var offer = new ServiceOffer
            {
                ServiceRequestId = requestId,
                ArtisanUserId = artisanUserId,
                Price = input.Price,
                Message = string.IsNullOrWhiteSpace(input.Message)
                    ? null
                    : input.Message.Trim(),
                EstimatedArrivalMinutes = input.EstimatedArrivalMinutes,
                Status = "Pending",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            var notification = new Notification
            {
                UserId = serviceRequest.CustomerUserId,
                Title = "وصلك عرض جديد",
                Message = $"أرسل أحد الصنايعية عرضًا جديدًا على طلبك رقم {serviceRequest.RequestNumber}. يمكنك مراجعة العرض الآن.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = now,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = serviceRequest.Id
            };

            _context.ServiceOffers.Add(offer);
            _context.Notifications.Add(notification);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "تعذر حفظ العرض. ربما أرسلت عرضًا على الطلب بالفعل."
                });
            }

            return StatusCode(201, new
            {
                message = "تم إرسال عرض السعر بنجاح.",
                offer = new
                {
                    offer.Id,
                    offer.ServiceRequestId,
                    serviceRequest.RequestNumber,
                    offer.ArtisanUserId,
                    offer.Price,
                    offer.Message,
                    offer.EstimatedArrivalMinutes,
                    offer.Status,
                    offer.CreatedAtUtc
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

        // GET: api/ArtisanOffers/my-offers
        // Artisan views all offers submitted by their account.
        [HttpGet("my-offers")]
        public async Task<IActionResult> GetMyOffers(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int artisanUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب الصنايعي من رمز الدخول."
                });
            }

            var offers = await _context.ServiceOffers
                .AsNoTracking()
                .Where(o => o.ArtisanUserId == artisanUserId)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(r => r.Specialty)
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => new
                {
                    o.Id,
                    o.ServiceRequestId,
                    o.ServiceRequest.RequestNumber,
                    SpecialtyName = o.ServiceRequest.Specialty.Name,
                    o.ServiceRequest.ProblemDescription,
                    RequestStatus = o.ServiceRequest.Status,
                    o.Price,
                    o.Message,
                    o.EstimatedArrivalMinutes,
                    o.Status,
                    o.CreatedAtUtc,
                    o.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(offers);
        }

        // POST: api/ArtisanOffers/requests/1/start
        // Artisan starts the work and the customer receives a notification.
        [HttpPost("requests/{requestId:int}/start")]
        public async Task<IActionResult> StartWork(
            int requestId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int artisanUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب الصنايعي."
                });
            }

            var artisan = await _context.Users
                .Include(u => u.ArtisanProfile)
                .FirstOrDefaultAsync(
                    u => u.Id == artisanUserId,
                    cancellationToken);

            if (artisan == null || artisan.Role != "Artisan")
            {
                return Forbid();
            }

            if (artisan.AccountStatus != "Active" ||
                artisan.ArtisanProfile == null ||
                artisan.ArtisanProfile.ApprovalStatus != "Approved")
            {
                return StatusCode(403, new
                {
                    message = "حساب الصنايعي غير مفعّل أو غير معتمد."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Make sure this artisan owns the accepted offer.
            var acceptedOffer = await _context.ServiceOffers
                .Where(o =>
                    o.ServiceRequestId == requestId &&
                    o.ArtisanUserId == artisanUserId &&
                    o.Status == "Accepted")
                .Select(o => new
                {
                    CustomerUserId = o.ServiceRequest.CustomerUserId,
                    RequestNumber = o.ServiceRequest.RequestNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (acceptedOffer == null)
            {
                return NotFound(new
                {
                    message = "لا يوجد عرض مقبول لهذا الطلب تابع لحسابك."
                });
            }

            var now = DateTime.UtcNow;

            var updatedRows = await _context.ServiceRequests
                .Where(r =>
                    r.Id == requestId &&
                    r.Status == "OfferAccepted")
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            r => r.Status,
                            "InProgress")
                        .SetProperty(
                            r => r.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (updatedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "لا يمكن بدء الطلب؛ ربما بدأ بالفعل أو تغيرت حالته."
                });
            }

            var notification = new Notification
            {
                UserId = acceptedOffer.CustomerUserId,
                Title = "بدأ الصنايعي تنفيذ طلبك",
                Message = $"بدأ الصنايعي تنفيذ الخدمة الخاصة بطلبك رقم {acceptedOffer.RequestNumber}.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = now,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = requestId
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "بدأ الصنايعي تنفيذ الطلب بنجاح.",
                requestId,
                requestStatus = "InProgress",
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

        // POST: api/ArtisanOffers/requests/1/finish
        // Artisan finishes the work and the customer receives a notification.
        [HttpPost("requests/{requestId:int}/finish")]
        public async Task<IActionResult> FinishWork(
            int requestId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int artisanUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب الصنايعي."
                });
            }

            var artisan = await _context.Users
                .Include(u => u.ArtisanProfile)
                .FirstOrDefaultAsync(
                    u => u.Id == artisanUserId,
                    cancellationToken);

            if (artisan == null || artisan.Role != "Artisan")
            {
                return Forbid();
            }

            if (artisan.AccountStatus != "Active" ||
                artisan.ArtisanProfile == null ||
                artisan.ArtisanProfile.ApprovalStatus != "Approved")
            {
                return StatusCode(403, new
                {
                    message = "حساب الصنايعي غير مفعّل أو غير معتمد."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Make sure this artisan owns the accepted offer.
            var acceptedOffer = await _context.ServiceOffers
                .Where(o =>
                    o.ServiceRequestId == requestId &&
                    o.ArtisanUserId == artisanUserId &&
                    o.Status == "Accepted")
                .Select(o => new
                {
                    CustomerUserId = o.ServiceRequest.CustomerUserId,
                    RequestNumber = o.ServiceRequest.RequestNumber
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (acceptedOffer == null)
            {
                return NotFound(new
                {
                    message = "لا يوجد عرض مقبول لهذا الطلب تابع لحسابك."
                });
            }

            var now = DateTime.UtcNow;

            var updatedRows = await _context.ServiceRequests
                .Where(r =>
                    r.Id == requestId &&
                    r.Status == "InProgress")
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            r => r.Status,
                            "AwaitingPayment")
                        .SetProperty(
                            r => r.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (updatedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "لا يمكن إنهاء الطلب؛ تأكد أنه قيد التنفيذ ولم تتغير حالته."
                });
            }

            var notification = new Notification
            {
                UserId = acceptedOffer.CustomerUserId,
                Title = "انتهى تنفيذ الخدمة",
                Message = $"انتهى الصنايعي من تنفيذ طلبك رقم {acceptedOffer.RequestNumber}. الطلب الآن في انتظار الدفع.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = now,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = requestId
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "تم إنهاء العمل بنجاح، والطلب الآن في انتظار الدفع.",
                requestId,
                requestStatus = "AwaitingPayment",
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

        // Extract user ID from JWT.
        private bool TryGetUserId(out int userId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out userId);
        }
    }
}
