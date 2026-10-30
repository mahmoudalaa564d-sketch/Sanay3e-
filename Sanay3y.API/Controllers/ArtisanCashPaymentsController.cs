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
    [Authorize(Roles = "Artisan")]
    public class ArtisanCashPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArtisanCashPaymentsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/ArtisanCashPayments/1/confirm-received
        // Artisan confirms receiving cash payment.
        [HttpPost("{paymentId:int}/confirm-received")]
        public async Task<IActionResult> ConfirmCashReceived(
            int paymentId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int artisanUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب الصنايعي."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Verify that the artisan account is active and approved.
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

            var payment = await _context.ServicePayments
                .Include(p => p.ServiceRequest)
                .FirstOrDefaultAsync(
                    p => p.Id == paymentId,
                    cancellationToken);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "عملية الدفع غير موجودة."
                });
            }

            // Verify that the current artisan owns the accepted offer.
            var isAcceptedArtisan = await _context.ServiceOffers
                .AnyAsync(
                    o => o.ServiceRequestId ==
                             payment.ServiceRequestId &&
                         o.ArtisanUserId == artisanUserId &&
                         o.Status == "Accepted",
                    cancellationToken);

            if (!isAcceptedArtisan)
            {
                return Forbid();
            }

            if (payment.PaymentMethod != "Cash")
            {
                return Conflict(new
                {
                    message = "تأكيد الاستلام النقدي متاح لعمليات الكاش فقط."
                });
            }

            if (payment.Status == "Paid")
            {
                return Ok(new
                {
                    message = "تم تأكيد الدفع بالفعل.",
                    paymentId = payment.Id,
                    paymentStatus = payment.Status,
                    requestStatus = payment.ServiceRequest.Status
                });
            }

            if (payment.Status != "AwaitingCashConfirmation")
            {
                return Conflict(new
                {
                    message = "عملية الدفع ليست في انتظار تأكيد الكاش."
                });
            }

            if (payment.ServiceRequest.Status != "AwaitingPayment")
            {
                return Conflict(new
                {
                    message = "حالة الطلب لا تسمح بتأكيد استلام المبلغ."
                });
            }

            var now = DateTime.UtcNow;

            if (payment.ArtisanCashConfirmedAtUtc == null)
            {
                payment.ArtisanCashConfirmedAtUtc = now;
            }

            // Payment is completed only after both parties confirm.
            if (payment.CustomerCashConfirmedAtUtc != null)
            {
                payment.Status = "Paid";
                payment.PaidAtUtc = now;

                payment.ServiceRequest.Status = "Completed";
                payment.ServiceRequest.UpdatedAtUtc = now;

                // Create notification for the customer.
                var notification = new Notification
                {
                    UserId = payment.ServiceRequest.CustomerUserId,
                    Title = "اكتملت خدمتك بنجاح",
                    Message = $"تم تأكيد استلام المبلغ النقدي واكتمال طلبك رقم {payment.ServiceRequest.RequestNumber}.",
                    Type = "Order",
                    IsRead = false,
                    CreatedAtUtc = now,
                    RelatedEntityType = "ServiceRequest",
                    RelatedEntityId = payment.ServiceRequest.Id
                };

                _context.Notifications.Add(notification);
            }

            payment.UpdatedAtUtc = now;

            // Save payment, request status, and notification together.
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = payment.Status == "Paid"
                    ? "تم تأكيد الدفع من الطرفين واكتمال الطلب."
                    : "تم تسجيل تأكيد الصنايعي. الطلب ينتظر تأكيد العميل.",

                paymentId = payment.Id,
                paymentStatus = payment.Status,
                requestStatus = payment.ServiceRequest.Status,

                customerConfirmed =
                    payment.CustomerCashConfirmedAtUtc != null,

                artisanConfirmed = true
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