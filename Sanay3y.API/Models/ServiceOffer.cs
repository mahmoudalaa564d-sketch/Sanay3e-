
using System;

namespace Sanay3y.API.Models
{
    public class ServiceOffer
    {
        public int Id { get; set; }

        // طلب الصيانة الذي يُقدَّم عليه العرض
        public int ServiceRequestId { get; set; }

        // حساب الصنايعي صاحب العرض
        public int ArtisanUserId { get; set; }

        // السعر المقترح للخدمة
        public decimal Price { get; set; }

        // رسالة أو تفاصيل إضافية للعميل
        public string? Message { get; set; }

        // الوقت المتوقع لوصول الصنايعي، بالدقائق
        public int? EstimatedArrivalMinutes { get; set; }

        // حالة العرض: Pending / Accepted / Rejected / Withdrawn
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // العلاقات
        public ServiceRequest ServiceRequest { get; set; } = null!;

        public User Artisan { get; set; } = null!;
    }
}
