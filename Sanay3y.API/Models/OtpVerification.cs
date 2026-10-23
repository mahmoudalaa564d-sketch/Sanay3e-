
using System;

namespace Sanay3y.API.Models
{
    public class OtpVerification
    {
        public int Id { get; set; }

        // المستخدم المطلوب تأكيد بريده أو هاتفه.
        public int UserId { get; set; }

        // Email أو Phone.
        public string Channel { get; set; } = string.Empty;

        // نخزن بصمة الكود، مش كود OTP نفسه.
        public string CodeHash { get; set; } = string.Empty;

        // وقت انتهاء صلاحية الكود.
        public DateTime ExpiresAtUtc { get; set; }

        // عدد المحاولات.
        public int Attempts { get; set; }

        // لو الكود اتستخدم، نسجل وقت استخدامه.
        public DateTime? UsedAtUtc { get; set; }

        // وقت إنشاء الكود.
        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // العلاقة بالمستخدم.
        public User User { get; set; } = null!;
    }
}
