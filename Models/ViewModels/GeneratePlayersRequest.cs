namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class GeneratePlayersRequest
    {
        public List<SimplePlayer> Players { get; set; } = new List<SimplePlayer>();
    }

    public class SimplePlayer
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NaturalPosition { get; set; } = string.Empty;
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//