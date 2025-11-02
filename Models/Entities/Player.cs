using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class Player
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Position NaturalPosition { get; set; }
        public PlayerRatings Ratings { get; set; } = new PlayerRatings();
        public bool IsCaptain { get; set; }
    }
}
