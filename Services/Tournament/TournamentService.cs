using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using TournamentEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Tournament;
using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;
// testing
using CountryEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Country;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Tournament
{
    public class TournamentService : ITournamentService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ICountryService _countryService;
        private readonly ILogger<TournamentService> _logger;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public TournamentService(IFirebaseService firebaseService, ICountryService countryService, ILogger<TournamentService> logger)
        {
            _firebaseService = firebaseService;
            _countryService = countryService;
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Create a new tournament with incremented tournament number
        public async Task<string> CreateNewTournamentAsync()
        {
            try
            {
                // Get latest tournament
                var latestTournament = await _firebaseService.GetMostRecentDocumentAsync<TournamentEntity>("tournaments", "tournamentNumber", 1);

                // Determine max tournament number
                var maxNumber = latestTournament?.TournamentNumber ?? 0;

                var tournament = new TournamentEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    TournamentNumber = maxNumber + 1, // increment number
                    Status = TournamentStatus.Registration.ToString(),
                    StartDate = DateTime.UtcNow,
                    RegisteredCountries = new List<string>(),
                    OriginalQuarterFinalCountries = new List<string>(),
                    Bracket = new TournamentBracket()
                };

                // Save to firestore
                await _firebaseService.AddDocumentWithIdAsync("tournaments", tournament.Id, tournament);

                _logger.LogInformation($"Created new tournament with ID: {tournament.Id}");

                return tournament.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new tournament");
                throw;
            }
        }

        // Get the most recent tournament or create a new one if none exist
        public async Task<TournamentEntity> GetOrCreateTournamentAsync()
        {
            try
            {
                // Get the most recent tournament
                var recentTournamentData = await _firebaseService.GetMostRecentDocumentWithIdAsync<TournamentEntity>("tournaments", "startDate", 1);

                // Check if tournament exists
                if (recentTournamentData.HasValue)
                {
                    var (tournament, documentId) = recentTournamentData.Value;
                    if (tournament != null)
                    {
                        tournament.Id = documentId;
                        _logger.LogInformation($"Found existing tournament with ID: {tournament.Id}");
                        return tournament;
                    }
                }

                // Create a new tournament if none exist
                _logger.LogInformation($"No tournaments found, creating new one");
                var tournamentId = await CreateNewTournamentAsync();
                var newTournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);
                
                return newTournament!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating tournament");
                throw;
            }
        }

        // Get the most recent tournament that is either in registration or in progress 
        public async Task<TournamentEntity?> GetCurrentActiveTournamentAsync()
        {
            try
            {
                // Get the most recent tournament
                var recentTournamentData = await _firebaseService.GetMostRecentDocumentWithIdAsync<TournamentEntity>("tournaments", "startDate", 1);

                if (recentTournamentData.HasValue)
                {
                    var (tournament, documentId) = recentTournamentData.Value;
                    if (tournament != null)
                    {
                        tournament.Id = documentId;
                        
                        // Check if it's in an active state
                        if (tournament.Status == TournamentStatus.Registration.ToString() || 
                            tournament.Status == TournamentStatus.InProgress.ToString())
                        {
                            return tournament;
                        }
                    }
                }

                // If most recent is not active, try querying for active statuses
                var registrationTournaments = await _firebaseService.QueryCollectionAsync<TournamentEntity>("tournaments", "status", TournamentStatus.Registration.ToString());
                if (registrationTournaments.Any())
                {
                    var mostRecentRegistration = registrationTournaments.OrderByDescending(t => t.StartDate).First();
                    return mostRecentRegistration;
                }

                // Query for most recent with in progress status
                var inProgressTournaments = await _firebaseService.QueryCollectionAsync<TournamentEntity>("tournaments", "status", TournamentStatus.InProgress.ToString());
                if (inProgressTournaments.Any())
                {
                    var mostRecentInProgress = inProgressTournaments.OrderByDescending(t => t.StartDate).First();
                    return mostRecentInProgress;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current active tournament");
                throw;
            }
        }

        // Check if tournament can start 
        public async Task<bool> CanTournamentStartAsync(string tournamentId)
        {
            try
            {
                var tournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);

                return tournament != null && tournament.RegisteredCountries.Count == 8;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tournament can start");
                return false;
            }
        }

        // Generate the tournament bracket and set status to InProgress
        public async Task GenerateBracketAsync(string tournamentId)
        {
            try
            {
                var tournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);

                if (tournament == null)
                {
                    throw new InvalidOperationException($"Tournament {tournamentId} not found");
                }

                if (tournament.RegisteredCountries.Count != 8)
                {
                    throw new InvalidOperationException($"Tournament must have exactly 8 teams to start. Currently has {tournament.RegisteredCountries.Count}");
                }

                // Verify all QF matches have both teams
                var quarterFinalMatches = new[] { "QF1", "QF2", "QF3", "QF4" };
                foreach (var matchId in quarterFinalMatches)
                {
                    if (!tournament.Matches.ContainsKey(matchId))
                    {
                        throw new InvalidOperationException($"Match {matchId} not found in tournament");
                    }

                    var match = tournament.Matches[matchId];
                    if (string.IsNullOrEmpty(match.HomeCountryId) || string.IsNullOrEmpty(match.AwayCountryId))
                    {
                        throw new InvalidOperationException($"Match {matchId} does not have both teams assigned");
                    }
                }

                // Save the original quarter-final countries when tournament starts (for easy restarts)
                if (tournament.OriginalQuarterFinalCountries == null || !tournament.OriginalQuarterFinalCountries.Any())
                {
                    tournament.OriginalQuarterFinalCountries = new List<string>(tournament.RegisteredCountries);
                    _logger.LogInformation($"Captured original quarter-final countries for tournament {tournamentId}: {string.Join(", ", tournament.OriginalQuarterFinalCountries)}");
                }

                // Set tournament status to in progress
                tournament.Status = TournamentStatus.InProgress.ToString();

                // Update tournament in Firestore
                await _firebaseService.UpdateDocumentAsync("tournaments", tournamentId, tournament);

                _logger.LogInformation($"Started tournament {tournamentId} with status InProgress");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting tournament {tournamentId}");
                throw;
            }
        }

        // Restarts and creates a new tournament using the last tournaments original quarter-final countries
        public async Task<TournamentEntity> RestartTournamentAsync()
        {
            try
            {
                // Get most recent tournament
                var recentTournamentData = await _firebaseService.GetMostRecentDocumentWithIdAsync<TournamentEntity>("tournaments", "startDate", 1);

                if (!recentTournamentData.HasValue)
                {
                    throw new InvalidOperationException("No previous tournament found to restart from");
                }

                var (tournament, documentId) = recentTournamentData.Value;
                if (tournament == null)
                {
                    throw new InvalidOperationException("No previous tournament found to restart from");
                }

                tournament.Id = documentId;
                var previousTournament = tournament;

                // Validate tournament has been started before allowing restart
                if (previousTournament.Status == TournamentStatus.Registration.ToString())
                {
                    throw new InvalidOperationException("Cannot restart tournament that is still in Registration status. Please start the tournament first.");
                }

                // Get the original QF countries from the previous tournament
                var originalCountries = previousTournament.OriginalQuarterFinalCountries?.Any() == true
                    ? previousTournament.OriginalQuarterFinalCountries
                    : previousTournament.RegisteredCountries;

                if (originalCountries == null || originalCountries.Count != 8)
                {
                    throw new InvalidOperationException($"Cannot restart tournament: need exactly 8 original countries, found {originalCountries?.Count ?? 0}");
                }

                // Mark the previous tournament as completed
                if (previousTournament.Status != TournamentStatus.Completed.ToString())
                {
                    previousTournament.Status = TournamentStatus.Completed.ToString();
                    previousTournament.EndDate = DateTime.UtcNow;

                    await _firebaseService.UpdateDocumentAsync("tournaments", previousTournament.Id, previousTournament);
                    _logger.LogInformation($"Marked previous tournament {previousTournament.Id} as completed");
                }

                // Get the latest tournament number
                var latestTournament = await _firebaseService.GetMostRecentDocumentAsync<TournamentEntity>("tournaments", "tournamentNumber", 1);
                var maxNumber = latestTournament?.TournamentNumber ?? 0;

                // Create new tournament
                var newTournament = new TournamentEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    TournamentNumber = maxNumber + 1, // increment number
                    Status = TournamentStatus.Registration.ToString(),
                    StartDate = DateTime.UtcNow,
                    RegisteredCountries = new List<string>(originalCountries),
                    OriginalQuarterFinalCountries = new List<string>(), 
                    Bracket = new TournamentBracket()
                };

                var quarterFinalMatches = new[] { "QF1", "QF2", "QF3", "QF4" };
                
                // Get the original match assignments from the previous tournament
                var previousQFMatches = previousTournament.Matches
                    .Where(m => quarterFinalMatches.Contains(m.Key))
                    .OrderBy(m => m.Key)
                    .ToList();

                if (previousQFMatches.Count == 4)
                {
                    // Recreate the QF matches with the original team assignments
                    foreach (var previousMatch in previousQFMatches)
                    {
                        var matchId = previousMatch.Key;
                        var previousMatchData = previousMatch.Value;

                        newTournament.Matches[matchId] = new MatchEntity
                        {
                            Id = matchId,
                            TournamentId = newTournament.Id,
                            Round = GetRoundFromMatchId(matchId),
                            Status = MatchStatus.Scheduled.ToString(),
                            MatchDate = DateTime.UtcNow,
                            HomeCountryId = previousMatchData.HomeCountryId,
                            AwayCountryId = previousMatchData.AwayCountryId
                        };

                        // Update tournament bracket
                        UpdateBracketWithMatch(newTournament.Bracket, matchId);

                        _logger.LogInformation($"Recreated match {matchId} with home={previousMatchData.HomeCountryId}, away={previousMatchData.AwayCountryId}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Previous tournament had {previousQFMatches.Count} QF matches, expected 4. Matches may not be properly recreated.");
                }

                // Save new tournament to firstore
                await _firebaseService.AddDocumentWithIdAsync("tournaments", newTournament.Id, newTournament);

                // Re-register all original countries
                var countriesToUpdate = await _countryService.GetCountriesByIdsAsync(originalCountries);
                
                // Update all countries in memory
                var updateTasks = new List<Task>();
                foreach (var countryId in originalCountries)
                {
                    if (countriesToUpdate.TryGetValue(countryId, out var country))
                    {
                        country.IsRegisteredForCurrentTournament = true;
                        updateTasks.Add(_firebaseService.UpdateDocumentAsync("countries", countryId, country));
                    }
                    else
                    {
                        _logger.LogWarning($"Country {countryId} not found when re-registering for tournament restart");
                    }
                }

                await Task.WhenAll(updateTasks);

                _logger.LogInformation($"Restarted tournament with ID: {newTournament.Id}, re-registered {updateTasks.Count} countries in their original positions");

                return newTournament;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting tournament");
                throw;
            }
        }

        // TESTING METHOD: Create a fully populated tournament with all existing countries (Used Claude to generate)
        //public async Task<TournamentEntity> CreateFullyPopulatedTournamentForTestingAsync()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Creating fully populated tournament for testing...");

        //        // Get all countries with their document IDs
        //        var allCountries = await _firebaseService.GetCollectionWithIdsAsync<CountryEntity>("countries");

        //        if (allCountries.Count != 8)
        //        {
        //            _logger.LogWarning($"Expected 8 countries, found {allCountries.Count}. Proceeding anyway...");
        //        }

        //        // Reset all countries' registration status to false (parallel updates)
        //        var resetTasks = new List<Task>();
        //        foreach (var (country, countryId) in allCountries)
        //        {
        //            country.IsRegisteredForCurrentTournament = false;
        //            resetTasks.Add(_firebaseService.UpdateDocumentAsync("countries", countryId, country));
        //        }
        //        await Task.WhenAll(resetTasks);

        //        _logger.LogInformation($"Reset registration status for {allCountries.Count} countries");

        //        // Create a new tournament
        //        var tournamentId = await CreateNewTournamentAsync();
        //        var tournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);

        //        if (tournament == null)
        //        {
        //            throw new Exception("Failed to create tournament");
        //        }

        //        // Add all countries to the tournament (2 per quarter-final match)
        //        var quarterFinalMatches = new[] { "QF1", "QF2", "QF3", "QF4" };
        //        var slots = new[] { "home", "away" };
                
        //        int countryIndex = 0;
        //        foreach (var matchId in quarterFinalMatches)
        //        {
        //            foreach (var slot in slots)
        //            {
        //                if (countryIndex < allCountries.Count)
        //                {
        //                    var (country, countryId) = allCountries[countryIndex];
                            
        //                    // Create or update match in tournament
        //                    if (!tournament.Matches.ContainsKey(matchId))
        //                    {
        //                        tournament.Matches[matchId] = new MatchEntity
        //                        {
        //                            Id = matchId,
        //                            TournamentId = tournament.Id,
        //                            Round = GetRoundFromMatchId(matchId),
        //                            Status = MatchStatus.Scheduled.ToString(),
        //                            MatchDate = DateTime.UtcNow,
        //                            HomeCountryId = string.Empty,
        //                            AwayCountryId = string.Empty
        //                        };
        //                    }

        //                    // Assign country to slot
        //                    if (slot == "home")
        //                    {
        //                        tournament.Matches[matchId].HomeCountryId = countryId;
        //                    }
        //                    else
        //                    {
        //                        tournament.Matches[matchId].AwayCountryId = countryId;
        //                    }

        //                    // Add to registered countries
        //                    if (!tournament.RegisteredCountries.Contains(countryId))
        //                    {
        //                        tournament.RegisteredCountries.Add(countryId);
        //                    }

        //                    // Update bracket
        //                    UpdateBracketWithMatch(tournament.Bracket, matchId);

        //                    // Update country registration status in memory (batch update later)
        //                    country.IsRegisteredForCurrentTournament = true;

        //                    _logger.LogInformation($"Added {country.Name} to {matchId} ({slot} slot)");

        //                    countryIndex++;
        //                }
        //            }
        //        }

        //        // Batch update all country registration statuses in parallel
        //        var registrationUpdateTasks = allCountries
        //            .Take(countryIndex)
        //            .Select(c => _firebaseService.UpdateDocumentAsync("countries", c.documentId, c.entity))
        //            .ToList();
        //        await Task.WhenAll(registrationUpdateTasks);

        //        // Save the fully populated tournament
        //        await _firebaseService.UpdateDocumentAsync("tournaments", tournament.Id, tournament);

        //        _logger.LogInformation($"Successfully created fully populated tournament with {tournament.RegisteredCountries.Count} countries");

        //        return tournament;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating fully populated tournament for testing");
        //        throw;
        //    }
        //}

        // Join an open match slot in the current tournament and update the tournament, match, and country data.
        public async Task<JoinTournamentResult> JoinTournamentSlotAsync(string matchId, string countryId, string slot)
        {
            try
            {
                var tournament = await GetCurrentActiveTournamentAsync();

                if (tournament == null)
                {
                    return new JoinTournamentResult
                    {
                        Success = false,
                        Message = "No active tournament available"
                    };
                }

                if (tournament.Status != TournamentStatus.Registration.ToString())
                {
                    return new JoinTournamentResult
                    {
                        Success = false,
                        Message = "Tournament is not accepting new registrations"
                    };
                }

                if (tournament.RegisteredCountries.Count >= 8)
                {
                    return new JoinTournamentResult
                    {
                        Success = false,
                        Message = "Tournament is full (8/8 teams registered)"
                    };
                }

                if (tournament.RegisteredCountries.Contains(countryId))
                {
                    return new JoinTournamentResult
                    {
                        Success = false,
                        Message = "Country is already registered for this tournament"
                    };
                }

                MatchEntity match;
                
                if (!tournament.Matches.ContainsKey(matchId))
                {
                    // Create the match if it doesn't exist
                    match = new MatchEntity
                    {
                        Id = matchId,
                        TournamentId = tournament.Id,
                        Round = GetRoundFromMatchId(matchId),
                        Status = MatchStatus.Scheduled.ToString(),
                        MatchDate = DateTime.UtcNow,
                        HomeCountryId = slot == "home" ? countryId : string.Empty,
                        AwayCountryId = slot == "away" ? countryId : string.Empty
                    };

                    tournament.Matches[matchId] = match;
                }
                else
                {
                    // Get existing match from tournament
                    match = tournament.Matches[matchId];
                    
                    // Update existing match
                    if (slot == "home")
                    {
                        if (!string.IsNullOrEmpty(match.HomeCountryId))
                        {
                            return new JoinTournamentResult
                            {
                                Success = false,
                                Message = "Home slot is already taken"
                            };
                        }
                        match.HomeCountryId = countryId;
                    }
                    else if (slot == "away")
                    {
                        if (!string.IsNullOrEmpty(match.AwayCountryId))
                        {
                            return new JoinTournamentResult
                            {
                                Success = false,
                                Message = "Away slot is already taken"
                            };
                        }
                        match.AwayCountryId = countryId;
                    }

                    tournament.Matches[matchId] = match;
                }

                // Add country to tournament's registered countries
                tournament.RegisteredCountries.Add(countryId);

                // Update bracket
                UpdateBracketWithMatch(tournament.Bracket, matchId);

                // Update tournament data in Firestore
                await _firebaseService.UpdateDocumentAsync("tournaments", tournament.Id, tournament);

                // Update country registration status
                await _countryService.UpdateTournamentRegistrationAsync(countryId, true);

                _logger.LogInformation("Country {CountryId} joined match {MatchId} as {Slot}",
                    countryId, matchId, slot);

                return new JoinTournamentResult
                {
                    Success = true,
                    Message = "Successfully joined tournament",
                    MatchName = GetMatchNameFromId(matchId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining tournament slot for match {MatchId}", matchId);
                return new JoinTournamentResult
                {
                    Success = false,
                    Message = "An error occurred while joining the tournament"
                };
            }
        }

        // Remove an admin-selected country from a tournament match slot during registration
        public async Task<RemoveCountryResult> RemoveCountryFromTournamentAsync(string matchId, string countryId, string slot)
        {
            try
            {
                var tournament = await GetCurrentActiveTournamentAsync();

                if (tournament == null)
                {
                    return new RemoveCountryResult
                    {
                        Success = false,
                        Message = "No active tournament available"
                    };
                }

                // Validate tournament is in registration phase
                if (tournament.Status != TournamentStatus.Registration.ToString())
                {
                    return new RemoveCountryResult
                    {
                        Success = false,
                        Message = "Cannot remove countries after tournament has started"
                    };
                }

                // Check if match exists
                if (!tournament.Matches.ContainsKey(matchId))
                {
                    return new RemoveCountryResult
                    {
                        Success = false,
                        Message = "Match not found"
                    };
                }

                var match = tournament.Matches[matchId];

                string removedCountryId = string.Empty;

                // Remove country from the selected slot
                if (slot == "home")
                {
                    if (match.HomeCountryId != countryId)
                    {
                        return new RemoveCountryResult
                        {
                            Success = false,
                            Message = "Country not found in home slot"
                        };
                    }
                    removedCountryId = match.HomeCountryId;
                    match.HomeCountryId = string.Empty;
                }
                else if (slot == "away")
                {
                    if (match.AwayCountryId != countryId)
                    {
                        return new RemoveCountryResult
                        {
                            Success = false,
                            Message = "Country not found in away slot"
                        };
                    }
                    removedCountryId = match.AwayCountryId;
                    match.AwayCountryId = string.Empty;
                }
                else
                {
                    return new RemoveCountryResult
                    {
                        Success = false,
                        Message = "Invalid slot specified. Must be 'home' or 'away'"
                    };
                }

                // Update match in tournament
                tournament.Matches[matchId] = match;

                // Remove country from tournament's registered countries
                tournament.RegisteredCountries.Remove(removedCountryId);

                // Update tournament in Firestore
                await _firebaseService.UpdateDocumentAsync("tournaments", tournament.Id, tournament);

                // Update country registration status
                await _countryService.UpdateTournamentRegistrationAsync(removedCountryId, false);

                _logger.LogInformation("Country {CountryId} removed from match {MatchId} ({Slot} slot)",
                    removedCountryId, matchId, slot);

                return new RemoveCountryResult
                {
                    Success = true,
                    Message = "Country successfully removed from tournament",
                    MatchName = GetMatchNameFromId(matchId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing country from match {MatchId}", matchId);
                return new RemoveCountryResult
                {
                    Success = false,
                    Message = "An error occurred while removing the country"
                };
            }
        }

        // Progress the winner of a completed match to the next round
        public async Task ProgressWinnerToNextRoundAsync(string tournamentId, string completedMatchId, string winnerId)
        {
            try
            {
                var tournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);

                if (tournament == null)
                {
                    throw new InvalidOperationException($"Tournament {tournamentId} not found");
                }

                _logger.LogInformation($"Progressing winner {winnerId} from match {completedMatchId} to next round");

                string? nextMatchId = null;
                string? nextSlot = null;

                // Determine next match and slot
                switch (completedMatchId)
                {
                    case "QF1":
                        nextMatchId = "SF1";
                        nextSlot = "home";
                        break;
                    case "QF2":
                        nextMatchId = "SF1";
                        nextSlot = "away";
                        break;
                    case "QF3":
                        nextMatchId = "SF2";
                        nextSlot = "home";
                        break;
                    case "QF4":
                        nextMatchId = "SF2";
                        nextSlot = "away";
                        break;
                    case "SF1":
                        nextMatchId = "Final";
                        nextSlot = "home";
                        break;
                    case "SF2":
                        nextMatchId = "Final";
                        nextSlot = "away";
                        break;
                    case "Final":
                        tournament.Status = TournamentStatus.Completed.ToString();
                        tournament.EndDate = DateTime.UtcNow;
                        _logger.LogInformation($"Tournament {tournamentId} completed. Winner: {winnerId}");
                        break;
                }

                if (!string.IsNullOrEmpty(nextMatchId) && !string.IsNullOrEmpty(nextSlot))
                {
                    if (!tournament.Matches.ContainsKey(nextMatchId))
                    {
                        // Create the next match
                        tournament.Matches[nextMatchId] = new MatchEntity
                        {
                            Id = nextMatchId,
                            TournamentId = tournament.Id,
                            Round = GetRoundFromMatchId(nextMatchId),
                            Status = MatchStatus.Scheduled.ToString(),
                            MatchDate = DateTime.UtcNow,
                            HomeCountryId = nextSlot == "home" ? winnerId : string.Empty,
                            AwayCountryId = nextSlot == "away" ? winnerId : string.Empty
                        };
                    }
                    else
                    {
                        // Update existing match
                        var nextMatch = tournament.Matches[nextMatchId];
                        if (nextSlot == "home")
                        {
                            nextMatch.HomeCountryId = winnerId;
                        }
                        else
                        {
                            nextMatch.AwayCountryId = winnerId;
                        }
                        tournament.Matches[nextMatchId] = nextMatch;
                    }

                    // Update bracket
                    UpdateBracketWithMatch(tournament.Bracket, nextMatchId);

                    _logger.LogInformation($"Winner {winnerId} progressed to {nextMatchId} ({nextSlot} slot)");
                }

                // Save tournament
                await _firebaseService.UpdateDocumentAsync("tournaments", tournamentId, tournament);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error progressing winner from match {completedMatchId}");
                throw;
            }
        }


        // HELPERS

        private string GetRoundFromMatchId(string matchId)
        {
            if (matchId.StartsWith("QF")) return MatchRound.QuarterFinal.ToString();
            if (matchId.StartsWith("SF")) return MatchRound.SemiFinal.ToString();
            if (matchId == "Final") return MatchRound.Final.ToString();

            throw new ArgumentException($"Invalid match ID format: {matchId}");
        }

        private string GetMatchNameFromId(string matchId)
        {
            var names = new Dictionary<string, string>
            {
                { "QF1", "Quarter Final 1" },
                { "QF2", "Quarter Final 2" },
                { "QF3", "Quarter Final 3" },
                { "QF4", "Quarter Final 4" },
                { "SF1", "Semi Final 1" },
                { "SF2", "Semi Final 2" },
                { "Final", "Final" }
            };

            return names.TryGetValue(matchId, out var name) ? name : matchId;
        }

        private void UpdateBracketWithMatch(TournamentBracket bracket, string matchId)
        {
            if (matchId.StartsWith("QF"))
            {
                if (!bracket.QuarterFinals.Contains(matchId))
                {
                    bracket.QuarterFinals.Add(matchId);
                }
            }
            else if (matchId.StartsWith("SF"))
            {
                if (!bracket.SemiFinals.Contains(matchId))
                {
                    bracket.SemiFinals.Add(matchId);
                }
            }
            else if (matchId == "Final")
            {
                bracket.Final = matchId;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//