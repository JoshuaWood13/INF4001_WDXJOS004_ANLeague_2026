using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator
{
    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        // TODO: Implement player generation logic
        // Generate 23 random players with names
        // Assign natural positions (2 GK, 8 DF, 8 MD, 5 AT)
        // Generate ratings based on natural position logic
        public Task<List<Player>> GeneratePlayersAsync(int count = 23)
        {
            throw new NotImplementedException();
        }
    }
}
