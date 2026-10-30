using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class VerifyPhoneChangeOtpDto
    {
        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string Code { get; set; } = string.Empty;
    }
}