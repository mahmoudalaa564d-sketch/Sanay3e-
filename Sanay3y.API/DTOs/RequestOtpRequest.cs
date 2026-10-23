
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class RequestOtpRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression(
            "^(Email|Phone)$",
            ErrorMessage = "Channel must be Email or Phone.")]
        public string Channel { get; set; } = string.Empty;
    }
}
