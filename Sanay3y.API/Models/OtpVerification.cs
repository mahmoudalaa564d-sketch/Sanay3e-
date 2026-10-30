
using System;

namespace Sanay3y.API.Models
{
    public class OtpVerification
    {
        public int Id { get; set; }

        // The user whose contact information is being verified.
        public int UserId { get; set; }

        // Email, Phone, or PhoneChange.
        public string Channel { get; set; } = string.Empty;

        // Hashed OTP code, never the original code.
        public string CodeHash { get; set; } = string.Empty;

        // Temporary target contact value, such as a new phone number.
        public string? TargetValue { get; set; }

        // OTP expiration time.
        public DateTime ExpiresAtUtc { get; set; }

        // Number of verification attempts.
        public int Attempts { get; set; }

        // Time when the OTP was used.
        public DateTime? UsedAtUtc { get; set; }

        // OTP creation time.
        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // Relationship with the user.
        public User User { get; set; } = null!;
    }
}
