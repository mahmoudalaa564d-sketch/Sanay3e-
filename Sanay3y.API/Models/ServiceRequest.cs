
namespace Sanay3y.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        // Unique, human-readable request number.
        public string RequestNumber { get; set; } = string.Empty;

        // Customer who created the request.
        public int CustomerUserId { get; set; }

        // Selected platform specialty, e.g. plumbing or carpentry.
        public int SpecialtyId { get; set; }

        // Selected service within the specialty.
        public string ServiceTypeName { get; set; } = string.Empty;

        // Problem details.
        public string ProblemDescription { get; set; } = string.Empty;

        public string? AdditionalNotes { get; set; }

        // Appointment information.
        public bool IsAsSoonAsPossible { get; set; }

        // Date/time should be sent with a timezone offset.
        public DateTimeOffset? ScheduledAtUtc { get; set; }

        // Address snapshot for this specific request.
        public string AddressTitle { get; set; } = string.Empty;

        public string Governorate { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? District { get; set; }

        public string Street { get; set; } = string.Empty;

        public string? BuildingNumber { get; set; }

        public string? FloorNumber { get; set; }

        public string? ApartmentNumber { get; set; }

        public string? Landmark { get; set; }

        // Optional coordinates, when the customer permits location access.
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Initial status: PendingOffers.
        public string Status { get; set; } = "PendingOffers";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Relationships.
        public User Customer { get; set; } = null!;

        public Specialty Specialty { get; set; } = null!;

        public ICollection<ServiceRequestAttachment> Attachments
        { get; set; } = new List<ServiceRequestAttachment>();
    }
}
