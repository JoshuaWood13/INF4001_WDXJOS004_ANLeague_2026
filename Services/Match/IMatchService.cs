using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public interface IMatchService
    {
        Task<MatchCommentaryResult> SimulateMatchAsync(string homeCountryId, string awayCountryId);
        Task<MatchCommentaryResult> PlayMatchAsync(string homeCountryId, string awayCountryId);
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//