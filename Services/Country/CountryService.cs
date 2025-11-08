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
                    Rating = 0,
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

        // Get country by ID
        public async Task<CountryEntity?> GetCountryByIdAsync(string countryId)
        {
            try
            {
                return await _firebaseService.GetDocumentAsync<CountryEntity>("countries", countryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting country by ID {countryId}");
                return null;
            }
        }

        // Get country by representative ID
        public async Task<CountryEntity?> GetCountryByRepresentativeIdAsync(string representativeId)
        {
            try
            {
                var countries = await _firebaseService.QueryCollectionAsync<CountryEntity>("countries", "federationRepresentativeId", representativeId);
                return countries.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting country by representative ID {representativeId}");
                return null;
            }
        }

        // Get country with document ID by representative ID
        public async Task<(CountryEntity? entity, string documentId)> GetCountryWithIdByRepresentativeIdAsync(string representativeId)
        {
            try
            {
                var countries = await _firebaseService.QueryCollectionWithIdsAsync<CountryEntity>("countries", "federationRepresentativeId", representativeId);
                var countryData = countries.FirstOrDefault();
                
                if (countryData.entity == null)
                {
                    return (null, string.Empty);
                }

                return (countryData.entity, countryData.documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting country with ID by representative ID {representativeId}");
                return (null, string.Empty);
            }
        }

        // Calculate rating for a country
        public Task<double> CalculateRatingAsync(CountryEntity country)
        {
            try
            {
                if (country.Players == null || country.Players.Count == 0)
                {
                    return Task.FromResult(0.0);
                }

                double totalRating = 0;
                int ratingCount = 0;

                foreach (var player in country.Players)
                {
                    totalRating += player.Ratings.GK;
                    totalRating += player.Ratings.DF;
                    totalRating += player.Ratings.MD;
                    totalRating += player.Ratings.AT;
                    ratingCount += 4; 
                }

                double averageRating = ratingCount > 0 ? totalRating / ratingCount : 0.0;

                return Task.FromResult(averageRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating average rating for country {country.Name}");
                return Task.FromResult(0.0);
            }
        }

        // Update an existing country in Firestore
        public async Task UpdateCountryAsync(string countryId, CountryEntity country)
        {
            try
            {
                await _firebaseService.UpdateDocumentAsync("countries", countryId, country);
                _logger.LogInformation($"Updated country: {country.Name} with ID: {countryId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating country {countryId}");
                throw;
            }
        }

        public Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon)
        {
            throw new NotImplementedException();
        }

        // Update tournament registration status for a country
        public async Task UpdateTournamentRegistrationAsync(string countryId, bool isRegistered)
        {
            try
            {
                var country = await GetCountryByIdAsync(countryId);
                
                if (country == null)
                {
                    _logger.LogWarning($"Country with ID {countryId} not found when updating tournament registration");
                    return;
                }

                country.IsRegisteredForCurrentTournament = isRegistered;
                await UpdateCountryAsync(countryId, country);

                _logger.LogInformation($"Updated tournament registration for country {country.Name} (ID: {countryId}) to {isRegistered}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating tournament registration for country ID {countryId}");
                throw;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//