
namespace Sanay3y.API.Models
{
    public class Specialty
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
