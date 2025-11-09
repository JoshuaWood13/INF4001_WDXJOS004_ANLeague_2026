using System.Diagnostics;
using INF4001_WDXJOS004_ANLeague_2026.Models;
using INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country; 
using Microsoft.AspNetCore.Mvc;
using TournamentEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Tournament;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFirebaseService _firebaseService;
        private readonly ICountryService _countryService;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public HomeController(ILogger<HomeController> logger, IFirebaseService firebaseService, ICountryService countryService)
        {
            _logger = logger;
            _firebaseService = firebaseService;
            _countryService = countryService;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Views
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public IActionResult Index()
        {
            return View();
        }

        // Get tournamnet and match data to display
        [HttpGet]
        public async Task<IActionResult> MatchHighlights(int? tournamentNumber = null)
        {
            try
            {
                // Get all tournaments and countries from Firebase
                var tournamentsTask = _firebaseService.GetCollectionAsync<TournamentEntity>("tournaments");
                var countriesTask = _firebaseService.GetCollectionWithIdsAsync<Models.Entities.Country>("countries");

                await Task.WhenAll(tournamentsTask, countriesTask);

                var tournaments = tournamentsTask.Result;
                var countries = countriesTask.Result;

                // Create country lookup
                var countryLookup = countries.ToDictionary(c => c.documentId, c => c.entity);
                
                // Sort tournaments
                var sortedTournaments = tournaments.OrderByDescending(t => t.TournamentNumber).ToList();

                var viewModel = new MatchHighlightsViewModel
                {
                    SelectedTournamentNumber = tournamentNumber
                };

                // Build view model
                foreach (var tournament in sortedTournaments)
                {
                    var tournamentViewModel = new TournamentMatchesViewModel
                    {
                        TournamentId = tournament.Id,
                        TournamentNumber = tournament.TournamentNumber,
                        Status = tournament.Status,
                        StartDate = tournament.StartDate,
                        EndDate = tournament.EndDate
                    };

                    // Get all completed matches
                    var completedMatches = tournament.Matches.Values
                        .Where(m => m.Status == "Completed")
                        .OrderBy(m => m.MatchDate)
                        .ToList();

                    // Build match cards
                    foreach (var match in completedMatches)
                    {
                        if (countryLookup.TryGetValue(match.HomeCountryId, out var homeCountry) &&
                            countryLookup.TryGetValue(match.AwayCountryId, out var awayCountry))
                        {
                            var matchCard = new MatchCardViewModel
                            {
                                MatchId = match.Id,
                                TournamentId = tournament.Id,
                                Round = match.Round,
                                Status = match.Status,
                                HomeCountryName = homeCountry.Name,
                                AwayCountryName = awayCountry.Name,
                                HomeScore = match.HomeScore,
                                AwayScore = match.AwayScore,
                                MatchType = match.MatchType,
                                MatchDate = match.MatchDate
                            };

                            tournamentViewModel.Matches.Add(matchCard);
                        }
                    }

                    if (tournamentViewModel.Matches.Any())
                    {
                        viewModel.Tournaments.Add(tournamentViewModel);
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading match highlights");
                return View(new MatchHighlightsViewModel());
            }
        }

        // Get data for a specific match to display
        [HttpGet]
        public async Task<IActionResult> MatchReplay(string tournamentId, string matchId)
        {
            try
            {
                if (string.IsNullOrEmpty(tournamentId) || string.IsNullOrEmpty(matchId))
                {
                    _logger.LogError("TournamentId and MatchId are required");
                    return RedirectToAction("MatchHighlights");
                }

                // Get tournament
                var tournament = await _firebaseService.GetDocumentAsync<TournamentEntity>("tournaments", tournamentId);

                if (tournament == null || !tournament.Matches.ContainsKey(matchId))
                {
                    _logger.LogError($"Match {matchId} not found in tournament {tournamentId}");
                    return RedirectToAction("MatchHighlights");
                }

                var match = tournament.Matches[matchId];

                if (match.Status != "Completed")
                {
                    _logger.LogError($"Match {matchId} is not completed");
                    return RedirectToAction("MatchHighlights");
                }

                // Get country names
                var homeCountry = await _countryService.GetCountryByIdAsync(match.HomeCountryId);
                var awayCountry = await _countryService.GetCountryByIdAsync(match.AwayCountryId);

                if (homeCountry == null || awayCountry == null)
                {
                    _logger.LogError($"Country data not found for match {matchId}");
                    return RedirectToAction("MatchHighlights");
                }

                // Build view model
                var viewModel = new MatchDetailViewModel
                {
                    Match = match,
                    HomeCountryName = homeCountry.Name,
                    AwayCountryName = awayCountry.Name,
                    MatchType = match.MatchType == "Played" 
                        ? Models.Enums.MatchType.Played 
                        : Models.Enums.MatchType.Simulated,
                    GoalScorers = match.GoalScorers ?? new List<Models.Entities.Goal>(),
                    Commentary = match.Commentary ?? new List<Models.Entities.CommentaryMoment>()
                };

                // Pass tournament number to view for back link
                ViewBag.TournamentNumber = tournament.TournamentNumber;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading match replay for {matchId}");
                return RedirectToAction("MatchHighlights");
            }
        }

        // Get top teams and top players
        [HttpGet]
        public async Task<IActionResult> Leaderboard()
        {
            try
            {
                var viewModel = await _countryService.GetLeaderboardDataAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading leaderboard");
                return View(new LeaderboardViewModel());
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//