using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class Match
    {
        public string Id { get; set; } = string.Empty;
        public string TournamentId { get; set; } = string.Empty;
        public MatchRound Round { get; set; }
        public string HomeCountryId { get; set; } = string.Empty;
        public string AwayCountryId { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public MatchStatus Status { get; set; }
        public Enums.MatchType MatchType { get; set; }
        public List<Goal> GoalScorers { get; set; } = new List<Goal>();
        public List<CommentaryMoment> Commentary { get; set; } = new List<CommentaryMoment>(); // Only for Played matches
        public DateTime MatchDate { get; set; }
        public string? WinnerId { get; set; } // nullable
    }
}
