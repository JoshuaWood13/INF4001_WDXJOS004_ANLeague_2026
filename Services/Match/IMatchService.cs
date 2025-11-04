using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public interface IMatchService
    {
        Task<MatchEntity> SimulateMatchAsync(string matchId);
        Task<MatchEntity> PlayMatchAsync(string matchId, Func<CommentaryMoment, Task>? commentaryCallback = null);
        Task<MatchEntity?> GetMatchByIdAsync(string matchId);
        Task<List<MatchEntity>> GetMatchesByTournamentAsync(string tournamentId);
    }
}
