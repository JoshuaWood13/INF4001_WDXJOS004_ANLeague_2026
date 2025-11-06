using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator
{
    public interface IPlayerGeneratorService
    {
        Task<List<Player>> GeneratePlayersAsync(int count = 23);
        Task<List<Player>> GeneratePlayersForPositionsAsync(IEnumerable<string> naturalPositions);
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//