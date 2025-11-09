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
        // Stream a played match with play-by-play AI commentary
        public async IAsyncEnumerable<CommentaryMoment> PlayMatchAsync(string homeCountryId, string awayCountryId)
        {
            _logger.LogInformation($"Playing match between {homeCountryId} and {awayCountryId}");

            // Get country data with players
            var countries = await _countryService.GetCountriesByIdsAsync(new List<string> { homeCountryId, awayCountryId });
            countries.TryGetValue(homeCountryId, out var homeCountry);
            countries.TryGetValue(awayCountryId, out var awayCountry);

            if (homeCountry == null || awayCountry == null)
            {
                var error = $"Country data not found - Home: {homeCountry?.Name ?? "NULL"}, Away: {awayCountry?.Name ?? "NULL"}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            // Stream commentary
            await foreach (var moment in _aiCommentaryService.StreamPlayMatchCommentaryAsync(
                homeCountryId,
                homeCountry.Name,
                homeCountry.Players,
                awayCountryId,
                awayCountry.Name,
                awayCountry.Players
            ))
            {
                yield return moment;
            }

            _logger.LogInformation($"Match completed streaming: {homeCountry.Name} vs {awayCountry.Name}");
        }

        // Stream a simulated match with simple, goal only AI generated commentary
        public async IAsyncEnumerable<CommentaryMoment> SimulateMatchAsync(string homeCountryId, string awayCountryId)
        {
            _logger.LogInformation($"Simulating match between {homeCountryId} and {awayCountryId}");

            // Get country data with players
            var countries = await _countryService.GetCountriesByIdsAsync(new List<string> { homeCountryId, awayCountryId });
            countries.TryGetValue(homeCountryId, out var homeCountry);
            countries.TryGetValue(awayCountryId, out var awayCountry);

            if (homeCountry == null || awayCountry == null)
            {
                var error = $"Country data not found - Home: {homeCountry?.Name ?? "NULL"}, Away: {awayCountry?.Name ?? "NULL"}";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            // Stream commentary from AI
            await foreach (var moment in _aiCommentaryService.StreamSimulateMatchCommentaryAsync(
                homeCountryId,
                homeCountry.Name,
                homeCountry.Players,
                awayCountryId,
                awayCountry.Name,
                awayCountry.Players
            ))
            {
                yield return moment;
            }

            _logger.LogInformation($"Match simulation completed streaming: {homeCountry.Name} vs {awayCountry.Name}");
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//