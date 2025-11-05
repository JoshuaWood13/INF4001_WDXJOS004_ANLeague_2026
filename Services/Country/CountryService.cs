using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using CountryEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Country;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Country
{
    public class CountryService : ICountryService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<CountryService> _logger;

        // Contructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public CountryService(IFirebaseService firebaseService, ILogger<CountryService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Check if a country is already registered in Firestore
        public async Task<bool> CheckIfRegisteredAsync(string countryName)
        {
            try
            {
                var countries = await _firebaseService.GetCollectionAsync<CountryEntity>("countries");
                return countries.Any(c => c.Name.Equals(countryName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if country {countryName} is registered");
                return false;
            }
        }

        // Create a new country in Firestore with only necessary initial data
        public async Task<string> CreateInitialCountryAsync(string countryName, string representativeId)
        {
            try
            {
                var country = new CountryEntity
                {
                    Name = countryName,
                    FederationRepresentativeId = representativeId,
                    ManagerName = null,
                    CaptainId = null,
                    Players = null,
                    AverageRating = 0,
                    IsRegisteredForCurrentTournament = false,
                    IsTeamComplete = false, 
                    Statistics = new TeamStatistics
                    {
                        TournamentsWon = 0,
                        MatchesPlayed = 0,
                        Wins = 0,
                        Losses = 0,
                        Draws = 0
                    },
                    CreatedAt = DateTime.UtcNow
                };

                // Add country to firestore
                var countryId = await _firebaseService.AddDocumentAsync("countries", country);

                _logger.LogInformation($"Created country: {countryName} with ID: {countryId}");

                return countryId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating initial country");
                throw;
            }
        }

        // Create a new country in Firestore
        public async Task<string> CreateCountryAsync(CountryEntity country)
        {
            try
            {
                var countryId = await _firebaseService.AddDocumentAsync("countries", country);
                _logger.LogInformation($"Created country: {country.Name} with ID: {countryId}");

                return countryId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating country");
                throw;
            }
        }

        public Task<CountryEntity?> GetCountryByIdAsync(string countryId)
        {
            throw new NotImplementedException();
        }

        public Task<CountryEntity?> GetCountryByRepresentativeIdAsync(string representativeId)
        {
            throw new NotImplementedException();
        }

        public Task<double> CalculateAverageRatingAsync(CountryEntity country)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon)
        {
            throw new NotImplementedException();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//