using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels;
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

        // Get multiple countries by their IDs
        public async Task<Dictionary<string, CountryEntity>> GetCountriesByIdsAsync(List<string> countryIds)
        {
            try
            {
                if (countryIds == null || !countryIds.Any())
                {
                    return new Dictionary<string, CountryEntity>();
                }

                var countryDict = await _firebaseService.GetDocumentsByIdsAsync<CountryEntity>("countries", countryIds);

                _logger.LogInformation($"Retrieved {countryDict.Count} countries out of {countryIds.Count} requested");

                return countryDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting countries by IDs");
                return new Dictionary<string, CountryEntity>();
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
                int playerCount = 0;

                foreach (var player in country.Players)
                {
                    // Only use the rating for the player's natural position
                    int positionRating = 0;
                    switch (player.NaturalPosition?.ToUpper())
                    {
                        case "GK":
                            positionRating = player.Ratings.GK;
                            break;
                        case "DF":
                            positionRating = player.Ratings.DF;
                            break;
                        case "MD":
                            positionRating = player.Ratings.MD;
                            break;
                        case "AT":
                            positionRating = player.Ratings.AT;
                            break;
                        default:
                            continue;
                    }

                    totalRating += positionRating;
                    playerCount++;
                }

                double averageRating = playerCount > 0 ? totalRating / playerCount : 0.0;
                
                double roundedRating = Math.Round(averageRating, MidpointRounding.AwayFromZero);

                return Task.FromResult(roundedRating);
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

        // Update team statistics after a match
        public async Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon)
        {
            try
            {
                var country = await GetCountryByIdAsync(countryId);

                if (country == null)
                {
                    _logger.LogWarning($"Country with ID {countryId} not found when updating team statistics");
                    return;
                }

                // Update statistics
                country.Statistics.MatchesPlayed++;

                if (won)
                {
                    country.Statistics.Wins++;
                }
                else if (drew)
                {
                    country.Statistics.Draws++;
                }
                else
                {
                    country.Statistics.Losses++;
                }

                if (tournamentWon)
                {
                    country.Statistics.TournamentsWon++;
                }

                await UpdateCountryAsync(countryId, country);

                _logger.LogInformation($"Updated statistics for {country.Name}: Played={country.Statistics.MatchesPlayed}, Wins={country.Statistics.Wins}, Losses={country.Statistics.Losses}, Draws={country.Statistics.Draws}, Tournaments={country.Statistics.TournamentsWon}");
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, $"Error updating team statistics for country ID {countryId}");
                throw;
            }
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

        // Update player goal counts after a match
        public async Task UpdatePlayerGoalsAsync(string countryId, Dictionary<string, int> playerGoalCounts)
        {
            try
            {
                var country = await GetCountryByIdAsync(countryId);

                if (country == null)
                {
                    _logger.LogWarning($"Country with ID {countryId} not found when updating player goals");
                    return;
                }

                if (country.Players == null || !country.Players.Any())
                {
                    _logger.LogWarning($"Country {country.Name} has no players to update");
                    return;
                }

                int updatedCount = 0;
                foreach (var playerGoal in playerGoalCounts)
                {
                    var player = country.Players.FirstOrDefault(p => p.Id == playerGoal.Key);
                    if (player != null)
                    {
                        player.goalsScored += playerGoal.Value;
                        updatedCount++;

                        _logger.LogInformation($"Updated {player.Name} goals: +{playerGoal.Value} (total: {player.goalsScored})");
                    }
                    else
                    {
                        _logger.LogWarning($"Player with ID {playerGoal.Key} not found in country {country.Name}");
                    }
                }

                await UpdateCountryAsync(countryId, country);

                _logger.LogInformation($"Updated goal counts for {updatedCount} players in {country.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating player goals for country ID {countryId}");
                throw;
            }
        }

        // Get leaderboard data for top teams and top players
        public async Task<LeaderboardViewModel> GetLeaderboardDataAsync()
        {
            try
            {
                // Fetch all countries with their IDs
                var countries = await _firebaseService.GetCollectionWithIdsAsync<CountryEntity>("countries");

                var viewModel = new LeaderboardViewModel();

                // Build top teams
                var topTeams = countries
                    .Select(c => new
                    {
                        Country = c.entity,
                        CountryId = c.documentId
                    })
                    .OrderByDescending(c => c.Country.Statistics.TournamentsWon)
                    .ThenByDescending(c => c.Country.Statistics.Wins)
                    .ThenBy(c => c.Country.Statistics.Losses)
                    .Take(5)
                    .Select((c, index) => new TeamLeaderboardEntry
                    {
                        Position = index + 1,
                        TeamName = c.Country.Name,
                        TournamentsWon = c.Country.Statistics.TournamentsWon,
                        Wins = c.Country.Statistics.Wins,
                        Losses = c.Country.Statistics.Losses
                    })
                    .ToList();

                viewModel.TopTeams = topTeams;

                // Build top players
                var allPlayers = countries
                    .Where(c => c.entity.Players != null && c.entity.Players.Any())
                    .SelectMany(c => c.entity.Players!
                        .Where(p => p.goalsScored > 0)
                        .Select(p => new
                        {
                            Player = p,
                            CountryName = c.entity.Name
                        }))
                    .OrderByDescending(p => p.Player.goalsScored)
                    .Take(10)
                    .Select((p, index) => new PlayerLeaderboardEntry
                    {
                        Position = index + 1,
                        PlayerName = p.Player.Name,
                        CountryName = p.CountryName,
                        Goals = p.Player.goalsScored
                    })
                    .ToList();

                viewModel.TopPlayers = allPlayers;

                _logger.LogInformation($"Retrieved leaderboard data: {topTeams.Count} teams, {allPlayers.Count} players");

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard data");
                throw;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//