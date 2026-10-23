
using System;
using System.Collections.Generic;

namespace Sanay3y.API.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        // The user can register using email or phone.
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        // Store a password hash, never a plain-text password.
        public string PasswordHash { get; set; } = string.Empty;

        // Customer, Artisan, or Admin.
        public string Role { get; set; } = "Customer";

        // PendingVerification, Active, or Suspended.
        public string AccountStatus { get; set; }
            = "PendingVerification";

        public bool IsEmailVerified { get; set; }

        public bool IsPhoneVerified { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // Navigation properties.
        public ArtisanProfile? ArtisanProfile { get; set; }

        public ICollection<OtpVerification> OtpVerifications
        { get; set; } = new List<OtpVerification>();

        public ICollection<ArtisanProfile> ReviewedArtisanProfiles
        { get; set; } = new List<ArtisanProfile>();
    }
}
