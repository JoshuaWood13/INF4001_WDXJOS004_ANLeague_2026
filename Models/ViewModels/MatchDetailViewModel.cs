using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class MatchDetailViewModel
    {
        public Match Match { get; set; } = new Match();
        public string HomeCountryName { get; set; } = string.Empty;
        public string AwayCountryName { get; set; } = string.Empty;
        public Enums.MatchType MatchType { get; set; }
        public List<Goal> GoalScorers { get; set; } = new List<Goal>();
        public List<CommentaryMoment> Commentary { get; set; } = new List<CommentaryMoment>();
    }
}
