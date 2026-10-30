using System;

namespace Sanay3y.API.Models
{
    public class CustomerAddress
    {
        public int Id { get; set; }

        public int CustomerUserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Governorate { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Street { get; set; } = string.Empty;

        public string? BuildingNumber { get; set; }

        public string? FloorNumber { get; set; }

        public string? ApartmentNumber { get; set; }

        public string? Landmark { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public User Customer { get; set; } = null!;
    }
}