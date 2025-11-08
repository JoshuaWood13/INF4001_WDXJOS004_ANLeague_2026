using TournamentEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Tournament;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Tournament
{
    public interface ITournamentService
    {
        Task<string> CreateNewTournamentAsync();
        Task<TournamentEntity> GetOrCreateTournamentAsync();
        Task<TournamentEntity?> GetCurrentActiveTournamentAsync();
        Task<bool> CanTournamentStartAsync(string tournamentId);
        Task GenerateBracketAsync(string tournamentId);
        Task<TournamentEntity> RestartTournamentAsync();
        Task<JoinTournamentResult> JoinTournamentSlotAsync(string matchId, string countryId, string slot);
        Task<RemoveCountryResult> RemoveCountryFromTournamentAsync(string matchId, string countryId, string slot);
        Task<TournamentEntity> CreateFullyPopulatedTournamentForTestingAsync();
        Task ProgressWinnerToNextRoundAsync(string tournamentId, string completedMatchId, string winnerId);
    }

    public class JoinTournamentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MatchName { get; set; } = string.Empty;
    }

    public class RemoveCountryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MatchName { get; set; } = string.Empty;
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//