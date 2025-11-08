using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels;
using INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using INF4001_WDXJOS004_ANLeague_2026.Services.Match;
using INF4001_WDXJOS004_ANLeague_2026.Services.Tournament;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ILogger<TournamentController> _logger;
        private readonly ICountryService _countryService;
        private readonly IFirebaseService _firebaseService;
        private readonly ITournamentService _tournamentService;
        private readonly IMatchService _matchService;
        private readonly IAICommentaryService _aiCommentaryService;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public TournamentController(ILogger<TournamentController> logger, ICountryService countryService, IFirebaseService firebaseService, ITournamentService tournamentService, IMatchService matchService, IAICommentaryService aiCommentaryService)
        {
            _logger = logger;
            _countryService = countryService;
            _firebaseService = firebaseService;
            _tournamentService = tournamentService;
            _matchService = matchService;
            _aiCommentaryService = aiCommentaryService;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Views
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Get tournament and user info to send to view 
        [HttpGet]
        public async Task<IActionResult> Tournament()
        {
            try
            {
                // Get or create tournament
                var tournament = await _tournamentService.GetOrCreateTournamentAsync();

                // Check auth and role
                bool isAuthenticated = User.Identity?.IsAuthenticated == true;
                bool isRepresentative = isAuthenticated && User.IsInRole("Representative");
                bool isAdmin = isAuthenticated && User.IsInRole("Administrator");

                string? userCountryId = null;
                string? userCountryName = null;
                bool isUserTeamRegistered = false;

                // Get representatives country info
                if (isRepresentative)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var (country, countryId) = await _countryService.GetCountryWithIdByRepresentativeIdAsync(userId);
                        if (country != null)
                        {
                            userCountryId = countryId;
                            userCountryName = country.Name;
                            isUserTeamRegistered = country.IsRegisteredForCurrentTournament;
                        }
                    }
                }

                // Tournament state checks
                bool isTournamentStarted = tournament.Status == "InProgress" || tournament.Status == "Completed";
                bool canTournamentStart = tournament.Status == "Registration" && tournament.RegisteredCountries.Count == 8;
                bool canTournamentRestart = tournament.Status == "InProgress" || tournament.Status == "Completed";
                bool showRemoveButtons = isAdmin && tournament.Status == "Registration";

                // Build match view models
                var matchViewModels = await BuildMatchViewModels(tournament.Matches.Values.ToList(), isTournamentStarted);
                
                // Get match or create empty one
                MatchViewModel GetOrCreateMatch(string matchId)
                {
                    var match = matchViewModels.FirstOrDefault(m => m.Id == matchId) 
                        ?? new MatchViewModel { Id = matchId, Status = "Scheduled", IsHomeSlotAvailable = true, IsAwaySlotAvailable = true };
                    
                    match.CanBePlayed = isTournamentStarted && 
                                       match.HomeCountry != null && 
                                       match.AwayCountry != null && 
                                       !match.IsMatchCompleted;
                    
                    return match;
                }

                // Build tournamnet view model
                var viewModel = new TournamentBracketViewModel
                {
                    // Tournament Info
                    TournamentId = tournament.Id,
                    TournamentNumber = tournament.TournamentNumber,
                    Status = tournament.Status,
                    RegisteredTeamsCount = tournament.RegisteredCountries.Count,
                    CanStart = canTournamentStart,
                    IsStarted = isTournamentStarted,
                    CanRestart = canTournamentRestart,
                    ShowRemoveButtons = showRemoveButtons,
                    
                    // User Info
                    IsAuthenticated = isAuthenticated,
                    IsRepresentative = isRepresentative,
                    IsAdmin = isAdmin,
                    IsUserTeamRegistered = isUserTeamRegistered,
                    UserCountryId = userCountryId,
                    UserCountryName = userCountryName,
                    
                    // Matches
                    QF1 = GetOrCreateMatch("QF1"),
                    QF2 = GetOrCreateMatch("QF2"),
                    QF3 = GetOrCreateMatch("QF3"),
                    QF4 = GetOrCreateMatch("QF4"),
                    SF1 = GetOrCreateMatch("SF1"),
                    SF2 = GetOrCreateMatch("SF2"),
                    Final = GetOrCreateMatch("Final")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tournament page");
                return View(new TournamentBracketViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> MatchDetails(string matchId, string mode)
        {
            try
            {
                if (string.IsNullOrEmpty(matchId))
                {
                    _logger.LogError("MatchId is required");
                    return RedirectToAction("Tournament");
                }

                // Get current tournament
                var tournament = await _tournamentService.GetOrCreateTournamentAsync();

                if (tournament == null || !tournament.Matches.ContainsKey(matchId))
                {
                    _logger.LogError($"Match {matchId} not found");
                    return RedirectToAction("Tournament");
                }

                var match = tournament.Matches[matchId];

                // Verify match has both teams
                if (string.IsNullOrEmpty(match.HomeCountryId) || string.IsNullOrEmpty(match.AwayCountryId))
                {
                    _logger.LogError($"Match {matchId} does not have both teams assigned");
                    return RedirectToAction("Tournament");
                }

                // Get country data with players
                var homeCountry = await _countryService.GetCountryByIdAsync(match.HomeCountryId);
                var awayCountry = await _countryService.GetCountryByIdAsync(match.AwayCountryId);

                if (homeCountry == null || awayCountry == null)
                {
                    _logger.LogError($"Could not find country data for match {matchId}");
                    return RedirectToAction("Tournament");
                }

                // Generate match result based on selected mode
                List<Models.Entities.CommentaryMoment> commentary = new List<Models.Entities.CommentaryMoment>();
                List<Models.Entities.Goal> goalScorers = new List<Models.Entities.Goal>();
                int homeScore = 0;
                int awayScore = 0;
                string matchType = "";

                if (mode == "play")
                {
                    // Generate AI commentary (placeholder implementation)
                    commentary = await _aiCommentaryService.GenerateMatchCommentaryAsync(homeCountry, awayCountry);
                    
                    // Extract scores and goals from commentary
                    var finalMoment = commentary.LastOrDefault();
                    if (finalMoment != null)
                    {
                        homeScore = finalMoment.HomeScore;
                        awayScore = finalMoment.AwayScore;
                    }

                    // Extract goals from commentary
                    goalScorers = commentary
                        .Where(c => c.Type == Models.Enums.CommentaryType.Goal)
                        .Select(c => new Models.Entities.Goal
                        {
                            PlayerName = c.PlayerName ?? "Unknown",
                            Minute = c.Minute,
                            PlayerId = c.PlayerId ?? string.Empty,
                            CountryId = c.HomeScore > (c.Minute > 45 ? homeScore : 0) ? match.HomeCountryId : match.AwayCountryId
                        })
                        .ToList();

                    matchType = "Played";

                    _logger.LogInformation($"Match {matchId} played with AI commentary");
                }
                else if (mode == "simulate")
                {
                    // Simulate match (placeholder implementation)
                    var result = await _matchService.SimulateMatchAsync(homeCountry, awayCountry);
                    homeScore = result.HomeScore;
                    awayScore = result.AwayScore;
                    goalScorers = result.GoalScorers;
                    matchType = "Simulated";
                    _logger.LogInformation($"Match {matchId} simulated");
                }
                else
                {
                    _logger.LogError($"Invalid mode: {mode}");
                    return RedirectToAction("Tournament");
                }

                // Determine winner
                string winnerId = homeScore > awayScore ? match.HomeCountryId : 
                                 awayScore > homeScore ? match.AwayCountryId : 
                                 string.Empty; // Draw (shouldn't happen in knockout)

                // Update match in tournament
                match.HomeScore = homeScore;
                match.AwayScore = awayScore;
                match.Status = "Completed";
                match.MatchType = matchType;
                match.WinnerId = winnerId;
                match.GoalScorers = goalScorers;
                match.Commentary = commentary;
                match.MatchDate = DateTime.UtcNow;

                tournament.Matches[matchId] = match;
                await _firebaseService.UpdateDocumentAsync("tournaments", tournament.Id, tournament);

                // Progress winner to next round
                if (!string.IsNullOrEmpty(winnerId))
                {
                    await _tournamentService.ProgressWinnerToNextRoundAsync(tournament.Id, matchId, winnerId);
                }

                // Build view model 
                var viewModel = new MatchDetailViewModel
                {
                    Match = match,
                    HomeCountryName = homeCountry.Name,
                    AwayCountryName = awayCountry.Name,
                    MatchType = matchType == "Played" ? Models.Enums.MatchType.Played : Models.Enums.MatchType.Simulated,
                    GoalScorers = goalScorers,
                    Commentary = commentary
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading match details for {matchId}");
                return RedirectToAction("Tournament");
            }
        }

        // Helper method to build match view models
        private async Task<List<MatchViewModel>> BuildMatchViewModels(List<Models.Entities.Match> matches, bool isTournamentStarted)
        {
            var matchViewModels = new List<MatchViewModel>();

            foreach (var match in matches)
            {
                var matchViewModel = new MatchViewModel
                {
                    Id = match.Id,
                    Status = match.Status,
                    HomeScore = match.HomeScore,
                    AwayScore = match.AwayScore,
                    WinnerId = match.WinnerId,
                    IsHomeSlotAvailable = string.IsNullOrEmpty(match.HomeCountryId),
                    IsAwaySlotAvailable = string.IsNullOrEmpty(match.AwayCountryId)
                };

                // Get home country info
                if (!string.IsNullOrEmpty(match.HomeCountryId))
                {
                    var homeCountry = await _countryService.GetCountryByIdAsync(match.HomeCountryId);
                    if (homeCountry != null)
                    {
                        matchViewModel.HomeCountry = new CountrySlotInfo
                        {
                            Id = match.HomeCountryId,
                            Name = homeCountry.Name,
                            ManagerName = homeCountry.ManagerName
                        };
                    }
                }

                // Get away country info
                if (!string.IsNullOrEmpty(match.AwayCountryId))
                {
                    var awayCountry = await _countryService.GetCountryByIdAsync(match.AwayCountryId);
                    if (awayCountry != null)
                    {
                        matchViewModel.AwayCountry = new CountrySlotInfo
                        {
                            Id = match.AwayCountryId,
                            Name = awayCountry.Name,
                            ManagerName = awayCountry.ManagerName
                        };
                    }
                }

                // Calculate if match can be played
                matchViewModel.CanBePlayed = isTournamentStarted &&
                                            matchViewModel.HomeCountry != null &&
                                            matchViewModel.AwayCountry != null &&
                                            !matchViewModel.IsMatchCompleted;

                matchViewModels.Add(matchViewModel);
            }

            return matchViewModels;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Get current or new tournamnet
        [HttpGet]
        public async Task<IActionResult> GetOrCreateTournament()
        {
            try
            {
                var tournament = await _tournamentService.GetOrCreateTournamentAsync();
                
                return Json(new
                {
                    success = true,
                    tournament = new
                    {
                        id = tournament.Id,
                        tournamentNumber = tournament.TournamentNumber,
                        status = tournament.Status.ToString(),
                        startDate = tournament.StartDate,
                        registeredCountries = tournament.RegisteredCountries,
                        originalQuarterFinalCountries = tournament.OriginalQuarterFinalCountries,
                        bracket = tournament.Bracket
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating tournament");
                return Json(new { success = false, message = "An error occurred while loading the tournament" });
            }
        }

        // Get all tournament matches
        [HttpGet]
        public async Task<IActionResult> GetTournamentMatches()
        {
            try
            {
                var tournament = await _tournamentService.GetOrCreateTournamentAsync();
                
                // Get all matches from tournament
                var matches = tournament.Matches.Values.ToList();

                return Json(new
                {
                    success = true,
                    matches = matches.Select(m => new
                    {
                        id = m.Id,
                        tournamentId = m.TournamentId,
                        round = m.Round,
                        status = m.Status,
                        homeCountryId = m.HomeCountryId,
                        awayCountryId = m.AwayCountryId,
                        homeScore = m.HomeScore,
                        awayScore = m.AwayScore,
                        winnerId = m.WinnerId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tournament matches");
                return Json(new { success = false, message = "An error occurred while loading matches" });
            }
        }

        // Join a user-selected tournament slot
        [HttpPost]
        [Authorize(Roles ="Representative")]
        public async Task<IActionResult> JoinTournament([FromBody] JoinTournamentRequest request)
        {
            try
            {
                // Validate request
                if (request == null || string.IsNullOrEmpty(request.MatchId) || string.IsNullOrEmpty(request.Slot))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                // Get user ID from claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Get country with document ID by representative ID
                var (country, countryId) = await _countryService.GetCountryWithIdByRepresentativeIdAsync(userId);

                if (country == null || string.IsNullOrEmpty(countryId))
                {
                    return Json(new { success = false, message = "No country found for this representative" });
                }

                // Check country has completed team setup
                if (!country.IsTeamComplete)
                {
                    return Json(new { success = false, message = "Please complete your team setup before joining a tournament" });
                }

                if (country.IsRegisteredForCurrentTournament)
                {
                    return Json(new { success = false, message = "Your country is already registered for the current tournament" });
                }

                // Validate slot parameter
                if (request.Slot != "home" && request.Slot != "away")
                {
                    return Json(new { success = false, message = "Invalid slot specified" });
                }

                // Join tournament slot
                var result = await _tournamentService.JoinTournamentSlotAsync(request.MatchId, countryId, request.Slot);

                if (result.Success)
                {
                    return Json(new {
                        success = true,
                        message = $"{country.Name} successfully joined {result.MatchName}",
                        countryId = countryId,
                        countryName = country.Name,
                        matchId = request.MatchId,
                        slot = request.Slot
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining tournament");
                return Json(new { success = false, message = "An error occurred while trying to join the tournament. Please try again later." });
            }
        }

        // Remove a selected country from a tournament slot during registration
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveCountryFromTournament([FromBody] RemoveCountryRequest request)
        {
            try
            {
                // Validate request
                if (request == null || string.IsNullOrEmpty(request.MatchId) || 
                    string.IsNullOrEmpty(request.CountryId) || string.IsNullOrEmpty(request.Slot))
                {
                    return Json(new { success = false, message = "Invalid request data" });
                }

                // Validate slot parameter
                if (request.Slot != "home" && request.Slot != "away")
                {
                    return Json(new { success = false, message = "Invalid slot specified" });
                }

                var result = await _tournamentService.RemoveCountryFromTournamentAsync(request.MatchId, request.CountryId, request.Slot);

                if (result.Success)
                {
                    var country = await _countryService.GetCountryByIdAsync(request.CountryId);
                    var countryName = country?.Name ?? "Country";

                    return Json(new
                    {
                        success = true,
                        message = $"{countryName} successfully removed from {result.MatchName}",
                        countryId = request.CountryId,
                        matchId = request.MatchId,
                        slot = request.Slot
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing country from tournament");
                return Json(new { success = false, message = "An error occurred while trying to remove the country. Please try again later." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Representative")]
        public async Task<IActionResult> GetUserCountryStatus()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var country = await _countryService.GetCountryByRepresentativeIdAsync(userId);

                if (country == null)
                {
                    return Json(new
                    {
                        success = true,
                        hasCountry = false,
                        isRegistered = false
                    });
                }

                return Json(new
                {
                    success = true,
                    hasCountry = true,
                    countryId = country.Name,
                    countryName = country.Name,
                    isTeamComplete = country.IsTeamComplete,
                    isRegistered = country.IsRegisteredForCurrentTournament
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user country status");
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCountryNames([FromBody] CountryNamesRequest request)
        {
            try
            {
                if (request == null || request.CountryIds == null || !request.CountryIds.Any())
                {
                    return Json(new { success = true, countryNames = new Dictionary<string, string>() });
                }

                var countryNames = new Dictionary<string, string>();

                foreach (var countryId in request.CountryIds)
                {
                    var country = await _countryService.GetCountryByIdAsync(countryId);
                    if (country != null)
                    {
                        countryNames[countryId] = country.Name;
                    }
                }

                return Json(new { success = true, countryNames });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting country names");
                return Json(new { success = false, message = "An error occurred while loading country names" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> StartTournament()
        {
            try
            {
                var tournament = await _tournamentService.GetOrCreateTournamentAsync();

                // Validate that the tournament can be started
                if (tournament == null)
                {
                    return Json(new { success = false, message = "No active tournament found" });
                }

                if (tournament.Status != "Registration")
                {
                    return Json(new { success = false, message = "Tournament has already been started" });
                }

                if (tournament.RegisteredCountries.Count != 8)
                {
                    return Json(new { success = false, message = $"Tournament requires exactly 8 teams to start. Currently has {tournament.RegisteredCountries.Count} teams." });
                }

                // Start the tournament 
                await _tournamentService.GenerateBracketAsync(tournament.Id);

                _logger.LogInformation($"Admin started tournament {tournament.Id}");

                return Json(new
                {
                    success = true,
                    message = "Tournament started successfully! Quarter-final matches are now ready to be played.",
                    tournament = new
                    {
                        id = tournament.Id,
                        tournamentNumber = tournament.TournamentNumber,
                        status = "InProgress"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting tournament");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RestartTournament()
        {
            try
            {
                var currentTournament = await _tournamentService.GetOrCreateTournamentAsync();

                // Validate that the tournament can be restarted
                if (currentTournament == null)
                {
                    return Json(new { success = false, message = "No active tournament found" });
                }

                if (currentTournament.Status == "Registration")
                {
                    return Json(new { success = false, message = "Cannot restart tournament that hasn't been started yet. Please start the tournament first." });
                }

                var newTournament = await _tournamentService.RestartTournamentAsync();

                return Json(new
                {
                    success = true,
                    message = $"Tournament restarted successfully. {newTournament.RegisteredCountries.Count} teams from the previous tournament have been re-registered.",
                    tournament = new
                    {
                        id = newTournament.Id,
                        tournamentNumber = newTournament.TournamentNumber,
                        status = newTournament.Status.ToString(),
                        registeredCountries = newTournament.RegisteredCountries,
                        originalQuarterFinalCountries = newTournament.OriginalQuarterFinalCountries
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting tournament");
                return Json(new { success = false, message = "An error occurred while restarting the tournament" });
            }
        }

        // Redirect to match details with the mode set to play
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PlayMatch(string matchId)
        {
            try
            {
                if (string.IsNullOrEmpty(matchId))
                {
                    return Json(new { success = false, message = "Match ID is required" });
                }

                _logger.LogInformation($"Admin initiated Play for match {matchId}");

                return RedirectToAction("MatchDetails", new { matchId = matchId, mode = "play" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating play for match {matchId}");
                return Json(new { success = false, message = "An error occurred while starting the match" });
            }
        }

        // Redirect to match details with the mode set to simulate
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SimulateMatch(string matchId)
        {
            try
            {
                if (string.IsNullOrEmpty(matchId))
                {
                    return Json(new { success = false, message = "Match ID is required" });
                }

                _logger.LogInformation($"Admin initiated Simulate for match {matchId}");

                return RedirectToAction("MatchDetails", new { matchId = matchId, mode = "simulate" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating simulation for match {matchId}");
                return Json(new { success = false, message = "An error occurred while simulating the match" });
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//