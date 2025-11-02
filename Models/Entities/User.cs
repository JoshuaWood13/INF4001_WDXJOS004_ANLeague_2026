using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? CountryId { get; set; } // nullable, for Representatives
        public DateTime CreatedAt { get; set; }
    }
}
