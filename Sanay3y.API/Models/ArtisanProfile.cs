
using System;
using System.Collections.Generic;

namespace Sanay3y.API.Models
{
    public class ArtisanProfile
    {
        public int UserId { get; set; }

        // Primary profession and experience.
        public string Profession { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }

        // Kept for compatibility with the existing application.
        public string ServiceArea { get; set; } = string.Empty;

        // Professional profile information.
        public string? Bio { get; set; }
        public string? ProfileImagePath { get; set; }

        // Private identity-document storage keys.
        public string? NationalIdFrontPath { get; set; }
        public string? NationalIdBackPath { get; set; }

        // Primary working location.
        public string? Governorate { get; set; }
        public string? Center { get; set; }
        public string? Village { get; set; }

        // Terms acceptance recorded by the server.
        public bool TermsAccepted { get; set; }
        public DateTime? TermsAcceptedAtUtc { get; set; }
        public string? TermsVersion { get; set; }

        // Admin approval status.
        public string ApprovalStatus { get; set; } = "Pending";
        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
        public string? RejectionReason { get; set; }

        // Relationships.
        public User User { get; set; } = null!;
        public User? ReviewedByAdmin { get; set; }

        public ICollection<ArtisanSpecialty> Specialties { get; set; }
            = new List<ArtisanSpecialty>();

        public ICollection<ArtisanServiceType> ServiceTypes { get; set; }
            = new List<ArtisanServiceType>();

        public ICollection<ArtisanServiceArea> NearbyServiceAreas { get; set; }
            = new List<ArtisanServiceArea>();
    }
}
