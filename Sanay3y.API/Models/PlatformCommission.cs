
using System;

namespace Sanay3y.API.Models
{
    public class PlatformCommission
    {
        public int Id { get; set; }

        // عملية الدفع التي نتجت عنها العمولة
        public int ServicePaymentId { get; set; }

        // الصنايعي صاحب الخدمة
        public int ArtisanUserId { get; set; }

        // مبلغ العمولة المستحق للمنصة
        public decimal Amount { get; set; }

        // Collected: العمولة تم الاحتفاظ بها للمنصة
        // Due: العمولة مستحقة على الصنايعي
        // Reversed: تم عكس العمولة
        public string Status { get; set; } = "Collected";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? CollectedAtUtc { get; set; }
    }
}
