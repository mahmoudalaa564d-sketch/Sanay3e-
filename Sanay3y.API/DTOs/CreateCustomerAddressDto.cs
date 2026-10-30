using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateCustomerAddressDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Governorate { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [StringLength(50)]
        public string? BuildingNumber { get; set; }

        [StringLength(50)]
        public string? FloorNumber { get; set; }

        [StringLength(50)]
        public string? ApartmentNumber { get; set; }

        [StringLength(250)]
        public string? Landmark { get; set; }

        [Range(typeof(decimal), "-90", "90")]
        public decimal? Latitude { get; set; }

        [Range(typeof(decimal), "-180", "180")]
        public decimal? Longitude { get; set; }

        public bool IsDefault { get; set; }
    }
}