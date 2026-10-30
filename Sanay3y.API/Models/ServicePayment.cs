
using System;

namespace Sanay3y.API.Models
{
    public class ServicePayment
    {
        public int Id { get; set; }

        // الطلب المرتبط بالدفع
        public int ServiceRequestId { get; set; }

        // العميل صاحب عملية الدفع
        public int CustomerUserId { get; set; }

        // إجمالي المبلغ الذي دفعه العميل
        public decimal Amount { get; set; }

        // نسبة عمولة المنصة وقت الدفع
        public decimal CommissionRate { get; set; }

        // قيمة عمولة المنصة
        public decimal CommissionAmount { get; set; }

        // صافي المبلغ المستحق للصنايعي
        public decimal ArtisanNetAmount { get; set; }

        public string Currency { get; set; } = "EGP";

        // Cash, Card, MobileWallet, Kiosk, Installments
        public string PaymentMethod { get; set; } = "Cash";

        // Manual أو Paymob أو InternalWallet
        public string Provider { get; set; } = "Manual";

        // Pending, AwaitingCustomerAction,
        // AwaitingCashConfirmation, Paid, Failed,
        // Cancelled, Expired, Refunded
        public string Status { get; set; } = "Pending";

        // بيانات مرجعية من بوابة الدفع
        public string? ProviderOrderId { get; set; }

        public string? ProviderTransactionId { get; set; }

        public string? CheckoutUrl { get; set; }

        public string? PaymentReference { get; set; }

        public string? FailureReason { get; set; }

        // تأكيدات الدفع النقدي
        public DateTime? CustomerCashConfirmedAtUtc { get; set; }

        public DateTime? ArtisanCashConfirmedAtUtc { get; set; }

        public DateTime? PaidAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // العلاقة بطلب الخدمة
        public ServiceRequest ServiceRequest { get; set; } = null!;
    }
}
