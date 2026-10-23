
using System;

namespace Sanay3y.API.Models
{
    public class ArtisanProfile
    {
        // نفس رقم المستخدم صاحب حساب الصنايعي.
        public int UserId { get; set; }

        // المهنة، مثل سباك أو كهربائي.
        public string Profession { get; set; } = string.Empty;

        // عدد سنوات الخبرة.
        public int ExperienceYears { get; set; }

        // المنطقة التي يقدم فيها الخدمة.
        public string ServiceArea { get; set; } = string.Empty;

        // حالة مراجعة الحساب بواسطة الإدارة.
        public string ApprovalStatus { get; set; } = "Pending";

        // رقم الأدمن الذي راجع الطلب، إن وُجد.
        public int? ReviewedByAdminId { get; set; }

        // وقت المراجعة.
        public DateTime? ReviewedAtUtc { get; set; }

        // سبب الرفض، إن وُجد.
        public string? RejectionReason { get; set; }

        // العلاقة بحساب الصنايعي نفسه.
        public User User { get; set; } = null!;

        // العلاقة بالأدمن الذي راجع الحساب.
        public User? ReviewedByAdmin { get; set; }
    }
}
