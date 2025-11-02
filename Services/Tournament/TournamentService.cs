using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Tournament
{
    public class TournamentService : ITournamentService
    {
        // TODO: Implement tournament management operations
        public Task<string> CreateNewTournamentAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Tournament?> GetCurrentActiveTournamentAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterCountryForTournamentAsync(string tournamentId, string countryId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CanTournamentStartAsync(string tournamentId)
        {
            throw new NotImplementedException();
        }

        public Task GenerateBracketAsync(string tournamentId)
        {
            throw new NotImplementedException();
        }

        public Task ProgressTournamentAsync(string tournamentId, string completedMatchId)
        {
            throw new NotImplementedException();
        }

        public Task RestartTournamentAsync()
        {
            throw new NotImplementedException();
        }
    }
}
