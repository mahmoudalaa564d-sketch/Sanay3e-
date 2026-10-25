
namespace Sanay3y.API.Models
{
    public class ArtisanSpecialty
    {
        public int Id { get; set; }
        public int ArtisanUserId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ArtisanProfile ArtisanProfile { get; set; } = null!;
    }
}
