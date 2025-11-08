namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class LeaderboardViewModel
    {
        public List<TeamLeaderboardEntry> TopTeams { get; set; } = new List<TeamLeaderboardEntry>();
        public List<PlayerLeaderboardEntry> TopPlayers { get; set; } = new List<PlayerLeaderboardEntry>();
    }

    public class TeamLeaderboardEntry
    {
        public int Position { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int TournamentsWon { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }

    public class PlayerLeaderboardEntry
    {
        public int Position { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int Goals { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//

