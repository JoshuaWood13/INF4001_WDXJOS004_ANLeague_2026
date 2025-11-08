using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary
{
    public interface IAICommentaryService
    {
        Task<MatchCommentaryResult> GetPlayMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers);
        Task<MatchCommentaryResult> GetSimulateMatchCommentaryAsync(string homeCountryId, string homeCountryName, List<Player> homePlayers, string awayCountryId, string awayCountryName, List<Player> awayPlayers);
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//