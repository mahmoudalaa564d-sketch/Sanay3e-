
using System;
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.Models
{
    public class Notification
    {
        public int Id { get; set; }

        // المستخدم الذي سيستقبل الإشعار
        public int UserId { get; set; }

        // عنوان الإشعار
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        // محتوى الإشعار
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        // نوع الإشعار: Order, Payment, Wallet, System
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "System";

        // هل المستخدم قرأ الإشعار؟
        public bool IsRead { get; set; } = false;

        // وقت إنشاء الإشعار
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // وقت قراءة الإشعار
        public DateTime? ReadAtUtc { get; set; }

        // بيانات اختيارية للعنصر المرتبط بالإشعار
        // مثال: ServiceRequest أو Payment
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}
