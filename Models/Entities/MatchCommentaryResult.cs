namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class MatchCommentaryResult
    {
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string WinnerId { get; set; } = string.Empty;
        public string HomeCountryName { get; set; } = string.Empty;
        public string AwayCountryName { get; set; } = string.Empty;

        public List<CommentaryMoment> Commentary { get; set; } = new List<CommentaryMoment>();
        public List<Goal> GoalScorers { get; set; } = new List<Goal>();
        public Dictionary<string, int> PlayerGoalCounts { get; set; } = new Dictionary<string, int>();
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//