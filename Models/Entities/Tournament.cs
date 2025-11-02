using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class Tournament
    {
        public string Id { get; set; } = string.Empty;
        public int TournamentNumber { get; set; } // auto-increment
        public TournamentStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // nullable
        public List<string> RegisteredCountries { get; set; } = new List<string>(); // Country IDs
        public TournamentBracket Bracket { get; set; } = new TournamentBracket();
    }
}
