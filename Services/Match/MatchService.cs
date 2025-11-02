using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public class MatchService : IMatchService
    {
        // TODO: Implement match simulation and play logic
        public Task<Match> SimulateMatchAsync(string matchId)
        {
            throw new NotImplementedException();
        }

        public Task<Match> PlayMatchAsync(string matchId, Func<CommentaryMoment, Task>? commentaryCallback = null)
        {
            throw new NotImplementedException();
        }

        public Task<Match?> GetMatchByIdAsync(string matchId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Match>> GetMatchesByTournamentAsync(string tournamentId)
        {
            throw new NotImplementedException();
        }
    }
}
