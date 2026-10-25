
namespace Sanay3y.API.Models
{
    public class ArtisanServiceType
    {
        public int Id { get; set; }
        public int ArtisanUserId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ArtisanProfile ArtisanProfile { get; set; } = null!;
    }
}
