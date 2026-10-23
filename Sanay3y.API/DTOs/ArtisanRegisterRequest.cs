
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profession is required.")]
        [StringLength(100)]
        public string Profession { get; set; } = string.Empty;

        [Range(0, 80, ErrorMessage = "Experience years must be between 0 and 80.")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Service area is required.")]
        [StringLength(200)]
        public string ServiceArea { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            bool hasEmail =
                !string.IsNullOrWhiteSpace(Email);

            bool hasPhone =
                !string.IsNullOrWhiteSpace(PhoneNumber);

            if (hasEmail == hasPhone)
            {
                yield return new ValidationResult(
                    "Provide either an email or a phone number, not both.",
                    new[] { nameof(Email), nameof(PhoneNumber) });
            }
        }
    }
}
