using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class TournamentBracketViewModel
    {
        public Tournament? Tournament { get; set; }
        public List<MatchViewModel> QuarterFinals { get; set; } = new List<MatchViewModel>();
        public List<MatchViewModel> SemiFinals { get; set; } = new List<MatchViewModel>();
        public MatchViewModel? Final { get; set; }
    }

    public class MatchViewModel
    {
        public string MatchId { get; set; } = string.Empty;
        public string? HomeCountryName { get; set; }
        public string? AwayCountryName { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public bool IsCompleted { get; set; }
    }
}
