
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class RegisterRequest : IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MinLength(8)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

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
