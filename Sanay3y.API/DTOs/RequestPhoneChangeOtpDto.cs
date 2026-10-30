using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class RequestPhoneChangeOtpDto
    {
        [Required]
        [StringLength(16, MinimumLength = 8)]
        [RegularExpression(
            @"^\+?[0-9]{8,15}$",
            ErrorMessage = "Enter a valid phone number.")]
        public string NewPhoneNumber { get; set; } = string.Empty;
    }
}