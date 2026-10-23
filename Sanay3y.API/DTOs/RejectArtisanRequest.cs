
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class RejectArtisanRequest
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [StringLength(
            500,
            MinimumLength = 5,
            ErrorMessage = "Rejection reason must be between 5 and 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
