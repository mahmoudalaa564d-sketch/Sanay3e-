
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Sanay3y.API.DTOs
{
    public class ArtisanRegisterRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Profession { get; set; } = string.Empty;

        [Range(0, 80)]
        public int ExperienceYears { get; set; }

        [Required]
        [StringLength(1500, MinimumLength = 20)]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profile image is required.")]
        public IFormFile? ProfileImage { get; set; }

        [Required(ErrorMessage = "Front image of national ID is required.")]
        public IFormFile? NationalIdFrontImage { get; set; }

        [Required(ErrorMessage = "Back image of national ID is required.")]
        public IFormFile? NationalIdBackImage { get; set; }

        [Required]
        [StringLength(100)]
        public string Governorate { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Center { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Village { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one specialty is required.")]
        public List<string> Specialties { get; set; } = new();

        [MinLength(1, ErrorMessage = "At least one service type is required.")]
        public List<string> ServiceTypes { get; set; } = new();

        // Additional areas where the artisan provides services.
        public List<ArtisanNearbyAreaRequest> NearbyServiceAreas
        { get; set; } = new();

        [Required]
        public bool TermsAccepted { get; set; }

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(Email);
            var hasPhone = !string.IsNullOrWhiteSpace(PhoneNumber);

            if (hasEmail == hasPhone)
            {
                yield return new ValidationResult(
                    "Provide either an email or a phone number, not both.",
                    new[] { nameof(Email), nameof(PhoneNumber) });
            }

            if (!TermsAccepted)
            {
                yield return new ValidationResult(
                    "You must accept the terms and conditions.",
                    new[] { nameof(TermsAccepted) });
            }

            if (HasInvalidList(Specialties, 10, 100))
            {
                yield return new ValidationResult(
                    "Provide between 1 and 10 valid, unique specialties.",
                    new[] { nameof(Specialties) });
            }

            if (HasInvalidList(ServiceTypes, 10, 100))
            {
                yield return new ValidationResult(
                    "Provide between 1 and 10 valid, unique service types.",
                    new[] { nameof(ServiceTypes) });
            }

            if (NearbyServiceAreas is null ||
                NearbyServiceAreas.Count > 20)
            {
                yield return new ValidationResult(
                    "You can provide up to 20 nearby service areas.",
                    new[] { nameof(NearbyServiceAreas) });
            }
            else
            {
                var seenAreas = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

                foreach (var area in NearbyServiceAreas)
                {
                    if (area is null ||
                        string.IsNullOrWhiteSpace(area.Governorate) ||
                        string.IsNullOrWhiteSpace(area.Center) ||
                        string.IsNullOrWhiteSpace(area.Village))
                    {
                        yield return new ValidationResult(
                            "Each nearby area must include its governorate, center, and village.",
                            new[] { nameof(NearbyServiceAreas) });

                        break;
                    }

                    var key = string.Join("|",
                        area.Governorate.Trim(),
                        area.Center.Trim(),
                        area.Village.Trim());

                    if (!seenAreas.Add(key))
                    {
                        yield return new ValidationResult(
                            "Nearby service areas must not be duplicated.",
                            new[] { nameof(NearbyServiceAreas) });

                        break;
                    }
                }
            }
        }

        private static bool HasInvalidList(
            List<string>? values,
            int maxCount,
            int maxLength)
        {
            if (values is null ||
                values.Count == 0 ||
                values.Count > maxCount)
            {
                return true;
            }

            var seen = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return true;
                }

                var trimmed = value.Trim();

                if (trimmed.Length > maxLength ||
                    !seen.Add(trimmed))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ArtisanNearbyAreaRequest
    {
        [Required]
        [StringLength(100)]
        public string Governorate { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Center { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Village { get; set; } = string.Empty;
    }
}
