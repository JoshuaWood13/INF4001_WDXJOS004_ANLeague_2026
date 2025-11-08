using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public class MatchService : IMatchService
    {
        private readonly ILogger<MatchService> _logger;
        private readonly ICountryService _countryService;
        private readonly IAICommentaryService _aiCommentaryService;

        // Controller 
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public MatchService(ILogger<MatchService> logger, ICountryService countryService, IAICommentaryService aiCommentaryService)
        {
            _logger = logger;
            _countryService = countryService;
            _aiCommentaryService = aiCommentaryService;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Play a match with detailed AI-generated commentary and pass back result to view
        public async Task<MatchCommentaryResult> PlayMatchAsync(string homeCountryId, string awayCountryId)
        {
            _logger.LogInformation($"Playing match between {homeCountryId} and {awayCountryId}");

            // Get country data with players
            var homeCountry = await _countryService.GetCountryByIdAsync(homeCountryId);
            var awayCountry = await _countryService.GetCountryByIdAsync(awayCountryId);

            if (homeCountry == null || awayCountry == null)
            {
                var error = $"Country data not found - Home: {homeCountry?.Name ?? "NULL"}, Away: {awayCountry?.Name ?? "NULL"}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            // Generate detailed commentary from AI
            var result = await _aiCommentaryService.GetPlayMatchCommentaryAsync(
                homeCountryId,
                homeCountry.Name,
                homeCountry.Players,
                awayCountryId,
                awayCountry.Name,
                awayCountry.Players
            );

            // Add country names to result
            result.HomeCountryName = homeCountry.Name;
            result.AwayCountryName = awayCountry.Name;

            _logger.LogInformation($"Match completed: {homeCountry.Name} {result.HomeScore}-{result.AwayScore} {awayCountry.Name}");

            return result;
        }

        // Simulate a match with simple AI-generated commentary and pass back result to view
        public async Task<MatchCommentaryResult> SimulateMatchAsync(string homeCountryId, string awayCountryId)
        {
            _logger.LogInformation($"Simulating match between {homeCountryId} and {awayCountryId}");

            // Get country data with players
            var homeCountry = await _countryService.GetCountryByIdAsync(homeCountryId);
            var awayCountry = await _countryService.GetCountryByIdAsync(awayCountryId);

            if (homeCountry == null || awayCountry == null)
            {
                var error = $"Country data not found - Home: {homeCountry?.Name ?? "NULL"}, Away: {awayCountry?.Name ?? "NULL"}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            // Generate result
            var result = await _aiCommentaryService.GetSimulateMatchCommentaryAsync(
                homeCountryId,
                homeCountry.Name,
                homeCountry.Players,
                awayCountryId,
                awayCountry.Name,
                awayCountry.Players
            );

            // Add country names to result
            result.HomeCountryName = homeCountry.Name;
            result.AwayCountryName = awayCountry.Name;

            _logger.LogInformation($"Match simulated: {homeCountry.Name} {result.HomeScore}-{result.AwayScore} {awayCountry.Name}");

            return result;
        }

        public Task<MatchEntity?> GetMatchByIdAsync(string matchId)
        {
            throw new NotImplementedException();
        }

        public Task<List<MatchEntity>> GetMatchesByTournamentAsync(string tournamentId)
        {
            throw new NotImplementedException();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//