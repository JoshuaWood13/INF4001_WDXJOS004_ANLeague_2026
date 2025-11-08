namespace INF4001_WDXJOS004_ANLeague_2026.Models.Entities
{
    public class JoinTournamentRequest
    {
        public string MatchId { get; set; } = string.Empty;
        public string Slot { get; set; } = string.Empty;
    }

    public class RemoveCountryRequest
    {
        public string MatchId { get; set; } = string.Empty;
        public string CountryId { get; set; } = string.Empty;
        public string Slot { get; set; } = string.Empty;
    }

    public class CountryNamesRequest
    {
        public List<string> CountryIds { get; set; } = new List<string>();
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//