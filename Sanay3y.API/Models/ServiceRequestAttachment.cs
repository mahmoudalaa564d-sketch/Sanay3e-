
namespace Sanay3y.API.Models
{
    public class ServiceRequestAttachment
    {
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        // Original filename supplied by the customer.
        public string OriginalFileName { get; set; } = string.Empty;

        // Server-generated filename used for storage.
        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ServiceRequest ServiceRequest { get; set; } = null!;
    }
}
