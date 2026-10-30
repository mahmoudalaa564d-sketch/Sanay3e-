
namespace Sanay3y.API.Models
{
    public class ArtisanServiceType
    {
        public int Id { get; set; }

        // The artisan who provides this service.
        public int ArtisanUserId { get; set; }

        // Service name, for example: AC Maintenance.
        public string Name { get; set; } = string.Empty;

        // Navigation property to the artisan profile.
        public ArtisanProfile ArtisanProfile { get; set; } = null!;
    }
}
