using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public interface IMatchService
    {
        Task<Match> SimulateMatchAsync(string matchId);
        Task<Match> PlayMatchAsync(string matchId, Func<CommentaryMoment, Task>? commentaryCallback = null);
        Task<Match?> GetMatchByIdAsync(string matchId);
        Task<List<Match>> GetMatchesByTournamentAsync(string tournamentId);
    }
}
