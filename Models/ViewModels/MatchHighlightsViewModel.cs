using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class MatchHighlightsViewModel
    {
        public List<TournamentMatchesViewModel> Tournaments { get; set; } = new List<TournamentMatchesViewModel>();
    }

    public class TournamentMatchesViewModel
    {
        public string TournamentId { get; set; } = string.Empty;
        public int TournamentNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<MatchCardViewModel> Matches { get; set; } = new List<MatchCardViewModel>();
    }

    public class MatchCardViewModel
    {
        public string MatchId { get; set; } = string.Empty;
        public string TournamentId { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string HomeCountryName { get; set; } = string.Empty;
        public string AwayCountryName { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string? MatchType { get; set; } 
        public DateTime MatchDate { get; set; }
        public bool IsCompleted => Status == "Completed";
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//

