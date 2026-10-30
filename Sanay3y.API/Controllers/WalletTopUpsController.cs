
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
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
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer,Artisan")]
    public class WalletTopUpsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WalletTopUpsController> _logger;

        public WalletTopUpsController(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WalletTopUpsController> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/WalletTopUps
        // إنشاء طلب شحن المحفظة
        [HttpPost]
        public async Task<IActionResult> CreateTopUp(
            [FromBody] CreateWalletTopUpDto input,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب المستخدم."
                });
            }

            if (input.Amount < 1m ||
                input.Amount > 10000m ||
                decimal.Round(input.Amount, 2) != input.Amount)
            {
                return BadRequest(new
                {
                    message = "مبلغ الشحن يجب أن يكون من 1 إلى 10000 جنيه وبحد أقصى منزلتين عشريتين."
                });
            }

            // تحميل إعدادات Paymob
            var secretKey =
                _configuration["Paymob:SecretKey"];

            var publicKey =
                _configuration["Paymob:PublicKey"];

            var notificationUrl =
                _configuration["Paymob:NotificationUrl"];

            var redirectionUrl =
                _configuration["Paymob:RedirectionUrl"];

            var integrationIds = _configuration
                .GetSection("Paymob:IntegrationIds")
                .GetChildren()
                .Select(item =>
                    int.TryParse(item.Value, out int id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            if (string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(publicKey) ||
                string.IsNullOrWhiteSpace(notificationUrl) ||
                !Uri.TryCreate(
                    notificationUrl,
                    UriKind.Absolute,
                    out var notificationUri) ||
                notificationUri.Scheme != Uri.UriSchemeHttps ||
                integrationIds.Length == 0)
            {
                return StatusCode(503, new
                {
                    message = "إعدادات Paymob غير مكتملة. تأكد من المفاتيح وIntegration IDs ورابط HTTPS للإشعارات."
                });
            }

            // لا نسمح بالشحن إلا لحساب نشط
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    u => u.Id == userId,
                    cancellationToken);

            if (user == null || user.AccountStatus != "Active")
            {
                return StatusCode(403, new
                {
                    message = "يجب أن يكون حسابك نشطًا لشحن المحفظة."
                });
            }

            if (string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return BadRequest(new
                {
                    message = "أضف بريدًا إلكترونيًا ورقم هاتف إلى حسابك قبل شحن المحفظة."
                });
            }

            // التأكد من وجود محفظة للمستخدم
            var wallet = await GetOrCreateWalletAsync(
                userId,
                cancellationToken);

            var now = DateTime.UtcNow;

            var topUp = new WalletTopUp
            {
                WalletId = wallet.Id,
                Amount = input.Amount,
                Currency = "EGP",
                Provider = "Paymob",
                SpecialReference =
                    $"SANAY3Y-TOPUP-{Guid.NewGuid():N}",
                Status = "Pending",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.WalletTopUps.Add(topUp);
            await _context.SaveChangesAsync(cancellationToken);

            // Paymob يتعامل بالقروش: 100 جنيه = 10000 قرش
            var amountInCents =
                decimal.ToInt64(input.Amount * 100m);

            var nameParts = user.FullName
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            var firstName =
                nameParts.FirstOrDefault() ?? "Customer";

            var lastName = nameParts.Length > 1
                ? string.Join(" ", nameParts.Skip(1))
                : "Customer";

            var payload = new Dictionary<string, object?>
            {
                ["amount"] = amountInCents,
                ["currency"] = "EGP",
                ["payment_methods"] = integrationIds,
                ["items"] = new[]
                {
                    new
                    {
                        name = "Sanay3y Wallet Top-up",
                        amount = amountInCents,
                        description = "Wallet balance top-up",
                        quantity = 1
                    }
                },
                ["billing_data"] = new
                {
                    apartment = "NA",
                    first_name = firstName,
                    last_name = lastName,
                    street = "NA",
                    building = "NA",
                    phone_number = user.PhoneNumber,
                    city = "Cairo",
                    country = "EG",
                    email = user.Email,
                    floor = "NA",
                    state = "Cairo"
                },
                ["extras"] = new
                {
                    wallet_top_up_id = topUp.Id,
                    wallet_user_id = userId
                },
                ["special_reference"] = topUp.SpecialReference,
                ["expiration"] = 3600,
                ["notification_url"] = notificationUrl
            };

            if (!string.IsNullOrWhiteSpace(redirectionUrl))
            {
                if (!Uri.TryCreate(
                        redirectionUrl,
                        UriKind.Absolute,
                        out var redirectUri) ||
                    redirectUri.Scheme != Uri.UriSchemeHttps)
                {
                    return StatusCode(503, new
                    {
                        message = "رابط الرجوع إلى الموقع يجب أن يكون HTTPS صالحًا."
                    });
                }

                payload["redirection_url"] = redirectionUrl;
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://accept.paymob.com/v1/intention/");

            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Token {secretKey}");

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(
                    request,
                    cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Could not contact Paymob for top-up {TopUpId}",
                    topUp.Id);

                // نترك العملية Pending؛ فقد يكون الطلب وصل إلى Paymob
                // رغم تعذر استلام الرد.
                return StatusCode(502, new
                {
                    message = "تعذر تأكيد الاتصال ببوابة الدفع. لا تعاود المحاولة قبل مراجعة العملية.",
                    topUpId = topUp.Id,
                    status = topUp.Status
                });
            }

            using (response)
            {
                var providerResponse =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    topUp.Status = "Failed";
                    topUp.UpdatedAtUtc = DateTime.UtcNow;

                    await _context.SaveChangesAsync(
                        cancellationToken);

                    _logger.LogWarning(
                        "Paymob returned HTTP {StatusCode} for top-up {TopUpId}.",
                        (int)response.StatusCode,
                        topUp.Id);

                    return StatusCode(502, new
                    {
                        message = "بوابة الدفع لم تقبل إنشاء عملية الشحن.",
                        topUpId = topUp.Id,
                        providerStatusCode =
                            (int)response.StatusCode
                    });
                }

                string? clientSecret;
                string? intentionId;
                string? orderId;

                try
                {
                    using var document =
                        JsonDocument.Parse(providerResponse);

                    var root = document.RootElement;

                    clientSecret = root.TryGetProperty(
                        "client_secret",
                        out var clientSecretElement)
                        ? clientSecretElement.GetString()
                        : null;

                    intentionId = root.TryGetProperty(
                        "id",
                        out var intentionElement)
                        ? intentionElement.ToString()
                        : null;

                    orderId = root.TryGetProperty(
                        "intention_order_id",
                        out var orderElement)
                        ? orderElement.ToString()
                        : null;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not parse Paymob response for top-up {TopUpId}",
                        topUp.Id);

                    topUp.Status = "Failed";
                    topUp.UpdatedAtUtc = DateTime.UtcNow;

                    await _context.SaveChangesAsync(
                        cancellationToken);

                    return StatusCode(502, new
                    {
                        message = "تعذر قراءة رد بوابة الدفع."
                    });
                }

                if (string.IsNullOrWhiteSpace(clientSecret) ||
                    string.IsNullOrWhiteSpace(intentionId) ||
                    string.IsNullOrWhiteSpace(orderId))
                {
                    topUp.Status = "Failed";
                    topUp.UpdatedAtUtc = DateTime.UtcNow;

                    await _context.SaveChangesAsync(
                        cancellationToken);

                    return StatusCode(502, new
                    {
                        message = "رد بوابة الدفع لا يحتوي على مراجع العملية المطلوبة."
                    });
                }

                topUp.ProviderIntentionId = intentionId;
                topUp.ProviderOrderId = orderId;
                topUp.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync(
                    cancellationToken);

                // رابط Unified Checkout باستخدام Public Key
                // وClient Secret الخاصين بهذه العملية
                var checkoutUrl =
                    "https://accept.paymob.com/unifiedcheckout/" +
                    $"?publicKey={Uri.EscapeDataString(publicKey)}" +
                    $"&clientSecret={Uri.EscapeDataString(clientSecret)}";

                return StatusCode(201, new
                {
                    message = "تم إنشاء طلب الشحن. أكمل الدفع من صفحة Paymob.",
                    topUpId = topUp.Id,
                    amount = topUp.Amount,
                    currency = topUp.Currency,
                    status = topUp.Status,
                    checkoutUrl
                });
            }
        }

        private async Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
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
                return newWallet;
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
