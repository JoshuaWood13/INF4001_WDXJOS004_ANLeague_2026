namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class Country
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // African country
        public string FederationRepresentativeId { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string CaptainId { get; set; } = string.Empty;
        public List<Player> Players { get; set; } = new List<Player>(); // 23 players
        public double AverageRating { get; set; } // calculated
        public TeamStatistics Statistics { get; set; } = new TeamStatistics();
        public bool IsRegisteredForCurrentTournament { get; set; }
    }
}
