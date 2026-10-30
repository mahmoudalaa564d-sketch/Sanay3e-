
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
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerPaymentsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/CustomerPayments/requests/1/payments
        // Customer creates a service payment.
        [HttpPost("requests/{requestId:int}/payments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreatePayment(
            int requestId,
            [FromBody] CreateServicePaymentDto input,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int customerUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            var paymentMethod = input.PaymentMethod.Trim();

            // Normalize payment method.
            if (paymentMethod.Equals(
                    "Cash",
                    StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = "Cash";
            }
            else if (paymentMethod.Equals(
                         "Card",
                         StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = "Card";
            }
            else if (paymentMethod.Equals(
                         "MobileWallet",
                         StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = "MobileWallet";
            }
            else if (paymentMethod.Equals(
                         "Kiosk",
                         StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = "Kiosk";
            }
            else if (paymentMethod.Equals(
                         "Installments",
                         StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = "Installments";
            }
            else
            {
                return BadRequest(new
                {
                    message = "طريقة الدفع غير مدعومة."
                });
            }

            // Verify request ownership.
            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(
                    r => r.Id == requestId &&
                         r.CustomerUserId == customerUserId,
                    cancellationToken);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message = "طلب الخدمة غير موجود أو لا يخص حسابك."
                });
            }

            // Payment is available after work completion.
            if (serviceRequest.Status != "AwaitingPayment")
            {
                return Conflict(new
                {
                    message = "الطلب ليس في حالة انتظار الدفع."
                });
            }

            // Get accepted offer price from database.
            var acceptedOfferPrice = await _context.ServiceOffers
                .Where(o =>
                    o.ServiceRequestId == requestId &&
                    o.Status == "Accepted")
                .Select(o => (decimal?)o.Price)
                .SingleOrDefaultAsync(cancellationToken);

            if (acceptedOfferPrice == null ||
                acceptedOfferPrice.Value <= 0)
            {
                return Conflict(new
                {
                    message = "لا يوجد عرض سعر مقبول صالح لهذا الطلب."
                });
            }

            // Electronic payment methods are not integrated yet.
            if (paymentMethod != "Cash")
            {
                return StatusCode(501, new
                {
                    message = "طريقة الدفع دي لم يتم ربطها ببوابة الدفع الإلكتروني بعد.",
                    paymentMethod,
                    nextStep = "PaymobIntegration"
                });
            }

            // Prevent multiple active payments.
            var hasActivePayment = await _context.ServicePayments
                .AnyAsync(
                    p => p.ServiceRequestId == requestId &&
                         (
                             p.Status == "Pending" ||
                             p.Status == "AwaitingCustomerAction" ||
                             p.Status == "AwaitingCashConfirmation"
                         ),
                    cancellationToken);

            if (hasActivePayment)
            {
                return Conflict(new
                {
                    message = "توجد عملية دفع نشطة لهذا الطلب بالفعل."
                });
            }

            var now = DateTime.UtcNow;

            var payment = new ServicePayment
            {
                ServiceRequestId = requestId,
                CustomerUserId = customerUserId,
                Amount = acceptedOfferPrice.Value,
                Currency = "EGP",
                PaymentMethod = "Cash",
                Provider = "Manual",
                Status = "AwaitingCashConfirmation",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.ServicePayments.Add(payment);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "تعذر إنشاء عملية الدفع. تحقق من حالة مدفوعات الطلب."
                });
            }

            return StatusCode(201, new
            {
                message = "تم إنشاء عملية الدفع النقدي بنجاح. في انتظار تأكيد العميل والصنايعي.",
                payment = new
                {
                    payment.Id,
                    payment.ServiceRequestId,
                    serviceRequest.RequestNumber,
                    payment.Amount,
                    payment.Currency,
                    payment.PaymentMethod,
                    payment.Status,
                    payment.CreatedAtUtc
                }
            });
        }

        // GET: api/CustomerPayments/my-payments
        // Get current customer's payment history.
        [HttpGet("my-payments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyPayments(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int customerUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            var payments = await _context.ServicePayments
                .AsNoTracking()
                .Where(p => p.CustomerUserId == customerUserId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => new
                {
                    p.Id,
                    p.ServiceRequestId,
                    p.ServiceRequest.RequestNumber,
                    p.Amount,
                    p.Currency,
                    p.PaymentMethod,
                    p.Provider,
                    p.Status,
                    p.PaidAtUtc,
                    p.CreatedAtUtc,
                    p.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(payments);
        }

        // POST: api/CustomerPayments/1/confirm-cash-paid
        // Customer confirms paying cash to the artisan.
        [HttpPost("{paymentId:int}/confirm-cash-paid")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ConfirmCashPaid(
            int paymentId,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int customerUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var payment = await _context.ServicePayments
                .Include(p => p.ServiceRequest)
                .FirstOrDefaultAsync(
                    p => p.Id == paymentId &&
                         p.CustomerUserId == customerUserId,
                    cancellationToken);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "عملية الدفع غير موجودة أو لا تخص حسابك."
                });
            }

            if (payment.PaymentMethod != "Cash")
            {
                return Conflict(new
                {
                    message = "تأكيد الدفع النقدي متاح لعمليات الكاش فقط."
                });
            }

            // Prevent duplicate processing after payment completion.
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
                    message = "حالة الطلب لا تسمح بتأكيد الدفع حاليًا."
                });
            }

            var now = DateTime.UtcNow;

            if (payment.CustomerCashConfirmedAtUtc == null)
            {
                payment.CustomerCashConfirmedAtUtc = now;
            }

            // Complete payment only after both parties confirm.
            if (payment.ArtisanCashConfirmedAtUtc != null)
            {
                payment.Status = "Paid";
                payment.PaidAtUtc = now;

                payment.ServiceRequest.Status = "Completed";
                payment.ServiceRequest.UpdatedAtUtc = now;

                // Notify customer when both parties have confirmed.
                var notification = new Notification
                {
                    UserId = payment.ServiceRequest.CustomerUserId,
                    Title = "اكتملت خدمتك بنجاح",
                    Message = $"تم تأكيد الدفع من الطرفين واكتمال طلبك رقم {payment.ServiceRequest.RequestNumber}.",
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
                    : "تم تسجيل تأكيد العميل. الطلب ينتظر تأكيد الصنايعي.",

                paymentId = payment.Id,
                paymentStatus = payment.Status,
                requestStatus = payment.ServiceRequest.Status,

                customerConfirmed = true,

                artisanConfirmed =
                    payment.ArtisanCashConfirmedAtUtc != null
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
