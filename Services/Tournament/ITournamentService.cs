using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Tournament
{
    public interface ITournamentService
    {
        Task<string> CreateNewTournamentAsync();
        Task<Tournament?> GetCurrentActiveTournamentAsync();
        Task<bool> RegisterCountryForTournamentAsync(string tournamentId, string countryId);
        Task<bool> CanTournamentStartAsync(string tournamentId);
        Task GenerateBracketAsync(string tournamentId);
        Task ProgressTournamentAsync(string tournamentId, string completedMatchId);
        Task RestartTournamentAsync();
    }
}
