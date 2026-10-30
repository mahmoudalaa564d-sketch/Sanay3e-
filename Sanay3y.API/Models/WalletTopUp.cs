
using System;

namespace Sanay3y.API.Models
{
    public class WalletTopUp
    {
        public int Id { get; set; }

        // المحفظة التي سيضاف إليها الرصيد
        public int WalletId { get; set; }

        // مبلغ الشحن بالجنيه المصري
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        // Paymob
        public string Provider { get; set; } = "Paymob";

        // مرجع فريد نستخدمه لربط إشعار الدفع بطلب الشحن
        public string SpecialReference { get; set; } = string.Empty;

        // بيانات مرجعية من Paymob
        public string? ProviderOrderId { get; set; }

        public string? ProviderIntentionId { get; set; }

        public string? ProviderTransactionId { get; set; }

        // Pending, Paid, Failed, Cancelled, Expired
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? PaidAtUtc { get; set; }

        public Wallet Wallet { get; set; } = null!;
    }
}
