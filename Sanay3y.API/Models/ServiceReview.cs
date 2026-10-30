
using System;

namespace Sanay3y.API.Models
{
    public class ServiceReview
    {
        public int Id { get; set; }

        // الطلب الذي تم تقييمه
        public int ServiceRequestId { get; set; }

        // العميل الذي كتب التقييم
        public int CustomerUserId { get; set; }

        // الصنايعي الذي تم تقييمه
        public int ArtisanUserId { get; set; }

        // التقييم من نجمة واحدة إلى خمس نجوم
        public int Rating { get; set; }

        // تعليق اختياري من العميل
        public string? Comment { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
