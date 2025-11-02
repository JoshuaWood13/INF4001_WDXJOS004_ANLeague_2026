namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class TournamentBracket
    {
        public List<string> QuarterFinals { get; set; } = new List<string>(); // 4 Match IDs
        public List<string> SemiFinals { get; set; } = new List<string>(); // 2 Match IDs
        public string Final { get; set; } = string.Empty; // Match ID
    }
}
