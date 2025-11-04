using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using TournamentEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Tournament;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Tournament
{
    public interface ITournamentService
    {
        Task<string> CreateNewTournamentAsync();
        Task<TournamentEntity?> GetCurrentActiveTournamentAsync();
        Task<bool> RegisterCountryForTournamentAsync(string tournamentId, string countryId);
        Task<bool> CanTournamentStartAsync(string tournamentId);
        Task GenerateBracketAsync(string tournamentId);
        Task ProgressTournamentAsync(string tournamentId, string completedMatchId);
        Task RestartTournamentAsync();
    }
}
