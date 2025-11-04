using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public class MatchService : IMatchService
    {
        // TODO: Implement match simulation and play logic
        public Task<MatchEntity> SimulateMatchAsync(string matchId)
        {
            throw new NotImplementedException();
        }

        public Task<MatchEntity> PlayMatchAsync(string matchId, Func<CommentaryMoment, Task>? commentaryCallback = null)
        {
            throw new NotImplementedException();
        }

        public Task<MatchEntity?> GetMatchByIdAsync(string matchId)
        {
            throw new NotImplementedException();
        }

        public Task<List<MatchEntity>> GetMatchesByTournamentAsync(string tournamentId)
        {
            throw new NotImplementedException();
        }
    }
}
