
using System;
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateServiceRequestDto
    {
        [Required]
        public int SpecialtyId { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceTypeName { get; set; } = string.Empty;

        [Required]
        [StringLength(3000, MinimumLength = 5)]
        public string ProblemDescription { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        public bool IsAsSoonAsPossible { get; set; }

        public DateTimeOffset? ScheduledAtUtc { get; set; }

        [Required]
        [StringLength(100)]
        public string AddressTitle { get; set; } = string.Empty;

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

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }
}
