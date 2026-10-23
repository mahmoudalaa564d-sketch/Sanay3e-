
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class LoginRequest
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
