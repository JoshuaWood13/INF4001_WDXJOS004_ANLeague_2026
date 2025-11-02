using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class CommentaryMoment
    {
        public int Minute { get; set; }
        public CommentaryType Type { get; set; }
        public string Description { get; set; } = string.Empty; // AI-generated
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? PlayerId { get; set; } // nullable, for Goal events
        public string? PlayerName { get; set; } // nullable
    }
}
