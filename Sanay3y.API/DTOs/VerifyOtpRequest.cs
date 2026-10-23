
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class VerifyOtpRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression(
            "^(Email|Phone)$",
            ErrorMessage = "Channel must be Email or Phone.")]
        public string Channel { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^[0-9]{6}$",
            ErrorMessage = "OTP must contain exactly 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }
}
