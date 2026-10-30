
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sanay3y.API.Data;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class WalletPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public WalletPaymentsController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/WalletPayments/requests/1/pay
        // دفع قيمة طلب الخدمة من محفظة العميل
        // مع توزيع المبلغ بين الصنايعي والمنصة
        [HttpPost("requests/{requestId:int}/pay")]
        public async Task<IActionResult> PayWithWallet(
            int requestId,
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

            var now = DateTime.UtcNow;

            // حجز الطلب مؤقتًا لمنع تنفيذ الدفع مرتين
            var claimedRows = await _context.ServiceRequests
                .Where(r =>
                    r.Id == requestId &&
                    r.CustomerUserId == customerUserId &&
                    r.Status == "AwaitingPayment")
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            r => r.Status,
                            "PaymentProcessing")
                        .SetProperty(
                            r => r.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (claimedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                var requestInfo = await _context.ServiceRequests
                    .AsNoTracking()
                    .Where(r =>
                        r.Id == requestId &&
                        r.CustomerUserId == customerUserId)
                    .Select(r => new
                    {
                        r.Id,
                        r.Status
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (requestInfo == null)
                {
                    return NotFound(new
                    {
                        message = "طلب الخدمة غير موجود أو لا يخص حسابك."
                    });
                }

                return Conflict(new
                {
                    message = "لا يمكن الدفع لهذا الطلب في حالته الحالية.",
                    requestStatus = requestInfo.Status
                });
            }

            // المبلغ والصنايعي يؤخذان من العرض المقبول في قاعدة البيانات
            var acceptedOffer = await _context.ServiceOffers
                .AsNoTracking()
                .Where(o =>
                    o.ServiceRequestId == requestId &&
                    o.Status == "Accepted")
                .Select(o => new
                {
                    o.Price,
                    o.ArtisanUserId
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (acceptedOffer == null || acceptedOffer.Price <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "لا يوجد عرض سعر مقبول صالح لهذا الطلب."
                });
            }

            // قراءة نسبة العمولة من إعدادات المشروع
            if (!TryGetCommissionRate(out decimal commissionRate))
            {
                await transaction.RollbackAsync(cancellationToken);

                return StatusCode(500, new
                {
                    message = "إعداد نسبة عمولة المنصة غير صحيح. يجب أن تكون أكبر من أو تساوي صفر وأقل من 100."
                });
            }

            // حساب عمولة المنصة وتقريبها إلى قرشين
            var commissionAmount = Math.Round(
                acceptedOffer.Price * commissionRate / 100m,
                2,
                MidpointRounding.AwayFromZero);

            // صافي المبلغ الذي سيصل إلى محفظة الصنايعي
            var artisanNetAmount =
                acceptedOffer.Price - commissionAmount;

            if (artisanNetAmount <= 0 || commissionAmount < 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return StatusCode(500, new
                {
                    message = "تعذر حساب توزيع مبلغ الخدمة بصورة صحيحة."
                });
            }

            var requestNumber = await _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Select(r => r.RequestNumber)
                .SingleAsync(cancellationToken);

            // لو توجد عملية كاش غير مكتملة، نلغيها عند اختيار الدفع من المحفظة.
            // لا نلغي تلقائيًا عملية دفع إلكترونية أخرى قيد التنفيذ.
            var activePayment = await _context.ServicePayments
                .FirstOrDefaultAsync(
                    p => p.ServiceRequestId == requestId &&
                         (
                             p.Status == "Pending" ||
                             p.Status == "AwaitingCustomerAction" ||
                             p.Status == "AwaitingCashConfirmation"
                         ),
                    cancellationToken);

            if (activePayment != null)
            {
                if (activePayment.PaymentMethod != "Cash")
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return Conflict(new
                    {
                        message = "توجد عملية دفع أخرى نشطة لهذا الطلب. أنهِها أو ألغها قبل استخدام المحفظة."
                    });
                }

                activePayment.Status = "Cancelled";
                activePayment.UpdatedAtUtc = now;

                await _context.SaveChangesAsync(cancellationToken);
            }

            // الحصول على محفظة العميل وإنشاؤها إن لم تكن موجودة
            var customerWallet = await GetOrCreateWalletAsync(
                customerUserId,
                cancellationToken);

            // الحصول على محفظة الصنايعي صاحب العرض المقبول
            var artisanWallet = await GetOrCreateWalletAsync(
                acceptedOffer.ArtisanUserId,
                cancellationToken);

            // التأكد من تطابق العملة
            if (customerWallet.Currency != "EGP" ||
                artisanWallet.Currency != "EGP")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "عملة إحدى المحافظ غير مدعومة لهذه العملية."
                });
            }

            // خصم إجمالي المبلغ من العميل
            var debitedRows = await _context.Wallets
                .Where(w =>
                    w.Id == customerWallet.Id &&
                    w.Balance >= acceptedOffer.Price)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            w => w.Balance,
                            w => w.Balance - acceptedOffer.Price)
                        .SetProperty(
                            w => w.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (debitedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "رصيد محفظتك غير كافٍ لإتمام عملية الدفع.",
                    requiredAmount = acceptedOffer.Price,
                    availableBalance = customerWallet.Balance,
                    currency = "EGP"
                });
            }

            // إضافة صافي المبلغ فقط إلى محفظة الصنايعي
            var creditedRows = await _context.Wallets
                .Where(w => w.Id == artisanWallet.Id)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            w => w.Balance,
                            w => w.Balance + artisanNetAmount)
                        .SetProperty(
                            w => w.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (creditedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return StatusCode(500, new
                {
                    message = "تعذرت إضافة المبلغ إلى محفظة الصنايعي. تم التراجع عن العملية."
                });
            }

            // تسجيل عملية الدفع مع تفاصيل العمولة
            var payment = new ServicePayment
            {
                ServiceRequestId = requestId,
                CustomerUserId = customerUserId,

                // إجمالي ما دفعه العميل
                Amount = acceptedOffer.Price,

                // بيانات توزيع المبلغ
                CommissionRate = commissionRate,
                CommissionAmount = commissionAmount,
                ArtisanNetAmount = artisanNetAmount,

                Currency = "EGP",
                PaymentMethod = "Wallet",
                Provider = "InternalWallet",
                Status = "Paid",
                PaidAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.ServicePayments.Add(payment);

            // حفظ الدفع للحصول على Payment ID
            await _context.SaveChangesAsync(cancellationToken);

            // رصيد المنصة بعد إضافة العمولة
            decimal? platformWalletBalanceAfter = null;

            // إضافة عمولة المنصة إلى حساب MAIN
            if (commissionAmount > 0)
            {
                // 1. تسجيل العمولة
                var platformCommission = new PlatformCommission
                {
                    ServicePaymentId = payment.Id,
                    ArtisanUserId = acceptedOffer.ArtisanUserId,
                    Amount = commissionAmount,
                    Status = "Collected",
                    CreatedAtUtc = now,
                    CollectedAtUtc = now
                };

                _context.PlatformCommissions.Add(platformCommission);

                // حفظ العمولة للحصول على Commission ID
                await _context.SaveChangesAsync(cancellationToken);

                // 2. الحصول على محفظة المنصة الرئيسية
                var platformWallet = await _context.PlatformWallets
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        w => w.AccountCode == "MAIN" &&
                             w.Currency == "EGP",
                        cancellationToken);

                if (platformWallet == null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return StatusCode(500, new
                    {
                        message = "محفظة المنصة الرئيسية غير موجودة. تعذر إتمام الدفع."
                    });
                }

                // 3. إضافة العمولة إلى رصيد المنصة بشكل ذري
                var platformCreditedRows = await _context.PlatformWallets
                    .Where(w =>
                        w.Id == platformWallet.Id &&
                        w.Currency == "EGP")
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                w => w.Balance,
                                w => w.Balance + commissionAmount)
                            .SetProperty(
                                w => w.UpdatedAtUtc,
                                now),
                        cancellationToken);

                if (platformCreditedRows == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return StatusCode(500, new
                    {
                        message = "تعذرت إضافة عمولة المنصة. تم التراجع عن عملية الدفع."
                    });
                }

                // 4. قراءة رصيد المنصة بعد إضافة العمولة
                platformWalletBalanceAfter =
                    await _context.PlatformWallets
                        .AsNoTracking()
                        .Where(w => w.Id == platformWallet.Id)
                        .Select(w => w.Balance)
                        .SingleAsync(cancellationToken);

                // 5. تسجيل حركة العمولة في سجل محفظة المنصة
                var platformWalletTransaction =
                    new PlatformWalletTransaction
                    {
                        PlatformWalletId = platformWallet.Id,

                        PlatformCommissionId = platformCommission.Id,

                        PlatformWithdrawalId = null,

                        Type = "CommissionCredit",
                        Direction = "Credit",

                        Amount = commissionAmount,

                        BalanceAfter = platformWalletBalanceAfter.Value,

                        Status = "Completed",

                        Description =
                            $"عمولة المنصة من عملية الدفع رقم {payment.Id}",

                        CreatedAtUtc = now
                    };

                _context.PlatformWalletTransactions.Add(
                    platformWalletTransaction);

                await _context.SaveChangesAsync(cancellationToken);
            }

            // تسجيل الخصم الكامل من محفظة العميل
            var customerTransaction = new WalletTransaction
            {
                WalletId = customerWallet.Id,
                ServicePaymentId = payment.Id,
                Type = "ServicePaymentDebit",
                Direction = "Debit",
                Amount = acceptedOffer.Price,
                BalanceAfter =
                    customerWallet.Balance - acceptedOffer.Price,
                Status = "Completed",
                Description = $"دفع طلب الخدمة {requestNumber}",
                Provider = "InternalWallet",
                CreatedAtUtc = now
            };

            // تسجيل صافي المبلغ المضاف إلى محفظة الصنايعي
            var artisanTransaction = new WalletTransaction
            {
                WalletId = artisanWallet.Id,
                ServicePaymentId = payment.Id,
                Type = "ServicePaymentCredit",
                Direction = "Credit",
                Amount = artisanNetAmount,
                BalanceAfter =
                    artisanWallet.Balance + artisanNetAmount,
                Status = "Completed",
                Description =
                    $"استلام صافي قيمة طلب الخدمة {requestNumber} بعد خصم عمولة المنصة",
                Provider = "InternalWallet",
                CreatedAtUtc = now
            };

            _context.WalletTransactions.Add(customerTransaction);
            _context.WalletTransactions.Add(artisanTransaction);

            await _context.SaveChangesAsync(cancellationToken);

            // إكمال الطلب بعد تسجيل الدفع والعمولة والقيود المالية
            var completedRows = await _context.ServiceRequests
                .Where(r =>
                    r.Id == requestId &&
                    r.CustomerUserId == customerUserId &&
                    r.Status == "PaymentProcessing")
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            r => r.Status,
                            "Completed")
                        .SetProperty(
                            r => r.UpdatedAtUtc,
                            now),
                    cancellationToken);

            if (completedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "تعذر إكمال الطلب. تم التراجع عن معاملة الدفع."
                });
            }

            // اعتماد العملية كلها معًا
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "تم الدفع من المحفظة بنجاح وإضافة عمولة المنصة.",

                requestId,
                paymentId = payment.Id,

                // إجمالي المبلغ
                amount = payment.Amount,

                // تفاصيل العمولة
                commissionRate = payment.CommissionRate,
                commissionAmount = payment.CommissionAmount,

                // صافي مستحق الصنايعي
                artisanNetAmount = payment.ArtisanNetAmount,

                currency = payment.Currency,
                paymentMethod = payment.PaymentMethod,
                paymentStatus = payment.Status,
                requestStatus = "Completed",

                // الأرصدة بعد العملية
                customerWalletBalance =
                    customerWallet.Balance - acceptedOffer.Price,

                artisanWalletBalance =
                    artisanWallet.Balance + artisanNetAmount,

                platformWalletBalance = platformWalletBalanceAfter
            });
        }

        // قراءة نسبة عمولة المنصة من الإعدادات
        private bool TryGetCommissionRate(out decimal rate)
        {
            var configuredRate =
                _configuration["PlatformCommission:Rate"];

            if (string.IsNullOrWhiteSpace(configuredRate))
            {
                rate = 10m;
                return true;
            }

            return decimal.TryParse(
                       configuredRate,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out rate)
                   && rate >= 0m
                   && rate < 100m;
        }

        // إنشاء محفظة برصيد صفر عند الحاجة
        private async Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.UserId == userId,
                    cancellationToken);

            if (wallet != null)
            {
                return wallet;
            }

            var now = DateTime.UtcNow;

            var newWallet = new Wallet
            {
                UserId = userId,
                Balance = 0m,
                Currency = "EGP",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.Wallets.Add(newWallet);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _context.Entry(newWallet).State =
                    EntityState.Detached;

                var existingWallet = await _context.Wallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w => w.UserId == userId,
                        cancellationToken);

                if (existingWallet != null)
                {
                    return existingWallet;
                }

                throw;
            }

            return await _context.Wallets
                .AsNoTracking()
                .FirstAsync(
                    w => w.UserId == userId,
                    cancellationToken);
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
