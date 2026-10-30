
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class UpdateCustomerProfileDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;
    }
}
