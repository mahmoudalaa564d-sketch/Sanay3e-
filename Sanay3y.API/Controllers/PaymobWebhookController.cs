
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sanay3y.API.Data;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PaymobWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymobWebhookController> _logger;

        public PaymobWebhookController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<PaymobWebhookController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/PaymobWebhook/transaction-processed
        // Paymob sends the verified transaction result to this endpoint.
        [HttpPost("transaction-processed")]
        public async Task<IActionResult> TransactionProcessed(
            CancellationToken cancellationToken)
        {
            var hmacSecret = _configuration["Paymob:HmacSecret"];

            if (string.IsNullOrWhiteSpace(hmacSecret))
            {
                _logger.LogError(
                    "Paymob HMAC secret is not configured.");

                return StatusCode(503, new
                {
                    message = "إعدادات التحقق من الدفع غير مكتملة."
                });
            }

            // Paymob sends the transaction HMAC in the query string.
            var receivedHmac = Request.Query["hmac"].ToString();

            if (string.IsNullOrWhiteSpace(receivedHmac))
            {
                return Unauthorized(new
                {
                    message = "إشعار الدفع لا يحتوي على توقيع صالح."
                });
            }

            JsonDocument document;

            try
            {
                document = await JsonDocument.ParseAsync(
                    Request.Body,
                    cancellationToken: cancellationToken);
            }
            catch (JsonException)
            {
                return BadRequest(new
                {
                    message = "تنسيق إشعار الدفع غير صالح."
                });
            }

            using (document)
            {
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object ||
                    !root.TryGetProperty("obj", out var obj) ||
                    obj.ValueKind != JsonValueKind.Object)
                {
                    return BadRequest(new
                    {
                        message = "بيانات المعاملة غير موجودة في الإشعار."
                    });
                }

                // Never trust a callback before HMAC verification.
                if (!TryBuildHmacData(obj, out var hmacData))
                {
                    return BadRequest(new
                    {
                        message = "بيانات التحقق من المعاملة غير مكتملة."
                    });
                }

                if (!VerifyHmac(
                        hmacData,
                        receivedHmac,
                        hmacSecret))
                {
                    _logger.LogWarning(
                        "Rejected Paymob callback because HMAC validation failed.");

                    return Unauthorized(new
                    {
                        message = "فشل التحقق من توقيع إشعار الدفع."
                    });
                }

                // Read transaction data only after validating the HMAC.
                var transactionId = GetValue(obj, "id");
                var orderId = GetValue(obj, "order", "id");
                var amountCentsText = GetValue(obj, "amount_cents");
                var currency = GetValue(obj, "currency");
                var integrationIdText = GetValue(obj, "integration_id");

                var merchantReference = FirstNonEmpty(
                    TryGetValue(obj, out var rootReference,
                        "merchant_order_id")
                        ? rootReference
                        : null,
                    TryGetValue(obj, out var orderReference,
                        "order", "merchant_order_id")
                        ? orderReference
                        : null);

                if (string.IsNullOrWhiteSpace(transactionId) ||
                    string.IsNullOrWhiteSpace(orderId) ||
                    string.IsNullOrWhiteSpace(amountCentsText) ||
                    string.IsNullOrWhiteSpace(currency) ||
                    !int.TryParse(
                        integrationIdText,
                        out int integrationId) ||
                    !decimal.TryParse(
                        amountCentsText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out decimal amountCents))
                {
                    return BadRequest(new
                    {
                        message = "بيانات المعاملة لا تحتوي على القيم المطلوبة."
                    });
                }

                // Only accept Integration IDs configured for this merchant.
                var allowedIntegrationIds = _configuration
                    .GetSection("Paymob:IntegrationIds")
                    .GetChildren()
                    .Select(item =>
                        int.TryParse(item.Value, out int id)
                            ? id
                            : 0)
                    .Where(id => id > 0)
                    .ToHashSet();

                if (allowedIntegrationIds.Count == 0)
                {
                    _logger.LogError(
                        "Paymob integration IDs are not configured.");

                    return StatusCode(503, new
                    {
                        message = "إعدادات طرق الدفع غير مكتملة."
                    });
                }

                if (!allowedIntegrationIds.Contains(integrationId))
                {
                    _logger.LogWarning(
                        "Rejected Paymob callback for unexpected integration ID.");

                    return Unauthorized(new
                    {
                        message = "معرّف طريقة الدفع غير معتمد لهذا التطبيق."
                    });
                }

                var success = ReadBoolean(obj, "success");
                var pending = ReadBoolean(obj, "pending");
                var errorOccurred = ReadBoolean(obj, "error_occured");
                var isVoided = ReadBoolean(obj, "is_voided");
                var isRefunded = ReadBoolean(obj, "is_refunded");

                // Begin the database transaction only after HMAC verification.
                await using var dbTransaction =
                    await _context.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.Serializable,
                        cancellationToken);

                WalletTopUp? topUp = null;

                // The Paymob order ID is the preferred correlation key.
                if (!string.IsNullOrWhiteSpace(orderId))
                {
                    topUp = await _context.WalletTopUps
                        .FirstOrDefaultAsync(
                            t => t.Provider == "Paymob" &&
                                 t.ProviderOrderId == orderId,
                            cancellationToken);
                }

                // Use the merchant reference as a fallback.
                if (topUp == null &&
                    !string.IsNullOrWhiteSpace(merchantReference))
                {
                    topUp = await _context.WalletTopUps
                        .FirstOrDefaultAsync(
                            t => t.Provider == "Paymob" &&
                                 t.SpecialReference == merchantReference,
                            cancellationToken);
                }

                if (topUp == null)
                {
                    _logger.LogWarning(
                        "No wallet top-up matched a verified Paymob order.");

                    await dbTransaction.RollbackAsync(cancellationToken);

                    return NotFound(new
                    {
                        message = "لم يتم العثور على طلب شحن مطابق لإشعار الدفع."
                    });
                }

                // Idempotency: a previously credited top-up must never
                // credit the wallet again.
                if (topUp.Status == "Paid")
                {
                    await dbTransaction.CommitAsync(cancellationToken);

                    return Ok(new
                    {
                        received = true,
                        message = "تمت معالجة طلب الشحن من قبل.",
                        topUpId = topUp.Id,
                        status = topUp.Status
                    });
                }

                // A browser redirect or a pending transaction is not proof
                // that payment succeeded.
                if (pending)
                {
                    await dbTransaction.CommitAsync(cancellationToken);

                    return Ok(new
                    {
                        received = true,
                        message = "تم استلام إشعار الدفع، والعملية ما زالت معلقة.",
                        topUpId = topUp.Id,
                        status = topUp.Status
                    });
                }

                // Do not credit failed, voided, or refunded transactions.
                if (!success || errorOccurred || isVoided || isRefunded)
                {
                    if (topUp.Status == "Pending")
                    {
                        topUp.Status = "Failed";
                        topUp.UpdatedAtUtc = DateTime.UtcNow;

                        await _context.SaveChangesAsync(
                            cancellationToken);
                    }

                    await dbTransaction.CommitAsync(cancellationToken);

                    return Ok(new
                    {
                        received = true,
                        message = "تم استلام إشعار معاملة لم تنجح، ولم تتم إضافة رصيد.",
                        topUpId = topUp.Id,
                        status = topUp.Status
                    });
                }

                // Do not automatically credit cancelled or expired requests.
                // A late successful payment needs reconciliation.
                if (topUp.Status != "Pending" &&
                    topUp.Status != "Failed")
                {
                    _logger.LogWarning(
                        "Successful Paymob transaction needs review for top-up {TopUpId} with status {Status}.",
                        topUp.Id,
                        topUp.Status);

                    await dbTransaction.RollbackAsync(cancellationToken);

                    return Conflict(new
                    {
                        message = "حالة طلب الشحن لا تسمح بإضافة الرصيد تلقائيًا، ويلزم مراجعة العملية.",
                        topUpId = topUp.Id,
                        status = topUp.Status
                    });
                }

                if (!string.Equals(
                        currency,
                        topUp.Currency,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Currency mismatch on Paymob top-up {TopUpId}.",
                        topUp.Id);

                    await dbTransaction.RollbackAsync(cancellationToken);

                    return Conflict(new
                    {
                        message = "عملة المعاملة لا تطابق عملة طلب الشحن."
                    });
                }

                var expectedAmountCents = topUp.Amount * 100m;

                if (amountCents != expectedAmountCents)
                {
                    _logger.LogWarning(
                        "Amount mismatch on Paymob top-up {TopUpId}.",
                        topUp.Id);

                    await dbTransaction.RollbackAsync(cancellationToken);

                    return Conflict(new
                    {
                        message = "المبلغ المدفوع لا يطابق مبلغ طلب الشحن."
                    });
                }

                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    await dbTransaction.RollbackAsync(cancellationToken);

                    return BadRequest(new
                    {
                        message = "رقم معاملة الدفع غير موجود."
                    });
                }

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(
                        w => w.Id == topUp.WalletId,
                        cancellationToken);

                if (wallet == null)
                {
                    _logger.LogError(
                        "Wallet not found for top-up {TopUpId}.",
                        topUp.Id);

                    await dbTransaction.RollbackAsync(cancellationToken);

                    return StatusCode(500, new
                    {
                        message = "محفظة طلب الشحن غير موجودة."
                    });
                }

                var now = DateTime.UtcNow;

                // Add the amount atomically inside the same DB transaction.
                var updatedWalletRows = await _context.Wallets
                    .Where(w => w.Id == wallet.Id)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(
                                w => w.Balance,
                                w => w.Balance + topUp.Amount)
                            .SetProperty(
                                w => w.UpdatedAtUtc,
                                now),
                        cancellationToken);

                if (updatedWalletRows != 1)
                {
                    await dbTransaction.RollbackAsync(cancellationToken);

                    return StatusCode(500, new
                    {
                        message = "تعذر تحديث رصيد المحفظة."
                    });
                }

                var newBalance = await _context.Wallets
                    .AsNoTracking()
                    .Where(w => w.Id == wallet.Id)
                    .Select(w => w.Balance)
                    .SingleAsync(cancellationToken);

                topUp.Status = "Paid";
                topUp.ProviderTransactionId = transactionId;
                topUp.PaidAtUtc = now;
                topUp.UpdatedAtUtc = now;

                // Keep a permanent ledger record for the top-up.
                var walletTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Type = "TopUp",
                    Direction = "Credit",
                    Amount = topUp.Amount,
                    BalanceAfter = newBalance,
                    Status = "Completed",
                    Description = $"شحن المحفظة - طلب رقم {topUp.Id}",
                    Provider = "Paymob",
                    ProviderTransactionId = transactionId,
                    CreatedAtUtc = now
                };

                _context.WalletTransactions.Add(walletTransaction);

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    await dbTransaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    await dbTransaction.RollbackAsync(cancellationToken);

                    _logger.LogError(
                        ex,
                        "Failed to commit verified Paymob top-up {TopUpId}.",
                        topUp.Id);

                    return StatusCode(500, new
                    {
                        message = "تعذر تسجيل عملية الشحن. سيتم فحص العملية قبل إعادة المحاولة."
                    });
                }

                return Ok(new
                {
                    received = true,
                    message = "تم تأكيد الدفع وإضافة الرصيد للمحفظة بنجاح.",
                    topUpId = topUp.Id,
                    amount = topUp.Amount,
                    currency = topUp.Currency,
                    transactionId,
                    walletBalance = newBalance,
                    status = topUp.Status
                });
            }
        }

        private static bool VerifyHmac(
            string data,
            string receivedHmac,
            string secret)
        {
            using var hmac = new HMACSHA512(
                Encoding.UTF8.GetBytes(secret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(data));

            var expectedHmac =
                Convert.ToHexString(hash).ToLowerInvariant();

            var provided = receivedHmac.Trim().ToLowerInvariant();

            var expectedBytes = Encoding.ASCII.GetBytes(expectedHmac);
            var providedBytes = Encoding.ASCII.GetBytes(provided);

            return expectedBytes.Length == providedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(
                       expectedBytes,
                       providedBytes);
        }

        // Paymob transaction callback HMAC uses these 20 fields
        // in exactly this order, concatenated without separators.
        private static bool TryBuildHmacData(
            JsonElement obj,
            out string data)
        {
            var fields = new[]
            {
                new[] { "amount_cents" },
                new[] { "created_at" },
                new[] { "currency" },
                new[] { "error_occured" },
                new[] { "has_parent_transaction" },
                new[] { "id" },
                new[] { "integration_id" },
                new[] { "is_3d_secure" },
                new[] { "is_auth" },
                new[] { "is_capture" },
                new[] { "is_refunded" },
                new[] { "is_standalone_payment" },
                new[] { "is_voided" },
                new[] { "order", "id" },
                new[] { "owner" },
                new[] { "pending" },
                new[] { "source_data", "pan" },
                new[] { "source_data", "sub_type" },
                new[] { "source_data", "type" },
                new[] { "success" }
            };

            var builder = new StringBuilder();

            foreach (var path in fields)
            {
                if (!TryGetValue(obj, out var value, path))
                {
                    data = string.Empty;
                    return false;
                }

                builder.Append(value);
            }

            data = builder.ToString();
            return true;
        }

        private static string GetValue(
            JsonElement element,
            params string[] path)
        {
            return TryGetValue(element, out var value, path)
                ? value
                : string.Empty;
        }

        private static bool TryGetValue(
            JsonElement element,
            out string value,
            params string[] path)
        {
            foreach (var part in path)
            {
                if (element.ValueKind != JsonValueKind.Object ||
                    !element.TryGetProperty(part, out element))
                {
                    value = string.Empty;
                    return false;
                }
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    value = element.GetString() ?? string.Empty;
                    return true;

                case JsonValueKind.Number:
                    value = element.GetRawText();
                    return true;

                case JsonValueKind.True:
                    value = "true";
                    return true;

                case JsonValueKind.False:
                    value = "false";
                    return true;

                case JsonValueKind.Null:
                    value = string.Empty;
                    return true;

                default:
                    value = string.Empty;
                    return false;
            }
        }

        private static bool ReadBoolean(
            JsonElement element,
            string propertyName)
        {
            var text = GetValue(element, propertyName);

            return bool.TryParse(text, out var result) && result;
        }

        private static string? FirstNonEmpty(
            params string?[] values)
        {
            return values.FirstOrDefault(
                value => !string.IsNullOrWhiteSpace(value));
        }
    }
}
