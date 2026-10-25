
namespace Sanay3y.API.Models
{
    public class ArtisanServiceArea
    {
        public int Id { get; set; }
        public int ArtisanUserId { get; set; }

        public string Governorate { get; set; } = string.Empty;
        public string Center { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;

        public ArtisanProfile ArtisanProfile { get; set; } = null!;
    }
}
