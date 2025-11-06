using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator;
using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    [Authorize(Roles = "Representative")]
    public class RepresentativeController : Controller
    {
        private readonly ILogger<RepresentativeController> _logger;
        private readonly ICountryService _countryService;
        private readonly IFirebaseService _firebaseService;
        private readonly IPlayerGeneratorService _playerGeneratorService;

        // Controller
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public RepresentativeController(ILogger<RepresentativeController> logger, ICountryService countryService, IFirebaseService firebaseService, IPlayerGeneratorService playerGeneratorService)
        {
            _logger = logger;
            _countryService = countryService;
            _firebaseService = firebaseService;
            _playerGeneratorService = playerGeneratorService;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Views
        //------------------------------------------------------------------------------------------------------------------------------------------//
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // TODO: Display tournament bracket with "Join Tournament" button if eligible
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyTeam()
        {
            try
            {
                // Get current user and country
                var (user, country, errorMessage) = await GetCurrentUserCountryAsync();

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (country == null)
                {
                    TempData["ErrorMessage"] = errorMessage ?? "Country not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Build view model
                var viewModel = new TeamDetailsViewModel
                {
                    Country = country
                };

                // Get captain name
                if (!string.IsNullOrEmpty(country.CaptainId) && country.Players != null)
                {
                    var captain = country.Players.FirstOrDefault(p => p.Id == country.CaptainId);

                    if (captain != null)
                    {
                        viewModel.CaptainName = captain.Name;
                    }
                }

                // Calculate W/L percentage
                if (country.Statistics.MatchesPlayed > 0)
                {
                    var totalMatches = country.Statistics.Wins + country.Statistics.Losses;

                    if (totalMatches > 0)
                    {
                        viewModel.WinLossPercentage = (double)country.Statistics.Wins / totalMatches * 100;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading MyTeam page");
                TempData["ErrorMessage"] = "An error occurred while loading your team details.";

                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> CreateTeam()
        {
            try
            {
                // Get current user and country
                var (user, country, errorMessage) = await GetCurrentUserCountryAsync();

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (country == null)
                {
                    TempData["ErrorMessage"] = errorMessage ?? "Country not found. Please contact support.";
                    return RedirectToAction("Index", "Home");
                }

                // Check if team is already complete
                if (country.IsTeamComplete)
                {
                    TempData["InfoMessage"] = "Your team is already complete.";
                    return RedirectToAction("MyTeam");
                }

                var viewModel = new CreateTeamViewModel
                {
                    CountryName = country.Name,
                    CountryId = user.CountryId,
                    Players = new List<Player>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CreateTeam page");
                TempData["ErrorMessage"] = "An error occurred while loading the create team page.";

                return RedirectToAction("MyTeam");
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(CreateTeamViewModel viewModel)
        {
            try
            {
                // Get current user and country
                var (user, country, errorMessage) = await GetCurrentUserCountryAsync();

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (country == null)
                {
                    ModelState.AddModelError("", errorMessage ?? "Country not found. Please contact support.");
                    return View(viewModel);
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    viewModel.CountryId = user.CountryId;
                    return View(viewModel);
                }

                // Validate players count
                if (viewModel.Players == null || viewModel.Players.Count != 23)
                {
                    ModelState.AddModelError("Players", "Team must have exactly 23 players.");
                    viewModel.CountryId = user.CountryId;
                    return View(viewModel);
                }

                // Validate positions
                var required = new Dictionary<string, int> { { "GK", 2 }, { "DF", 8 }, { "MD", 8 }, { "AT", 5 } };
                var counts = new Dictionary<string, int> { { "GK", 0 }, { "DF", 0 }, { "MD", 0 }, { "AT", 0 } };

                foreach (var p in viewModel.Players)
                {
                    if (p.NaturalPosition != null && counts.ContainsKey(p.NaturalPosition))
                    {
                        counts[p.NaturalPosition]++;
                    }
                }

                foreach (var kv in required)
                {
                    if (counts[kv.Key] < kv.Value)
                    {
                        ModelState.AddModelError("Players", "Squad must include at least 2 GK, 8 DF, 8 MD, and 5 AT.");
                        viewModel.CountryId = user.CountryId;

                        return View(viewModel);
                    }
                }

                // Validate captain selection
                if (string.IsNullOrEmpty(viewModel.CaptainId))
                {
                    ModelState.AddModelError("CaptainId", "Please select a captain.");
                    viewModel.CountryId = user.CountryId;

                    return View(viewModel);
                }

                // Check captain exists in players list
                if (!viewModel.Players.Any(p => p.Id == viewModel.CaptainId))
                {
                    ModelState.AddModelError("CaptainId", "Selected captain must be in the player list.");
                    viewModel.CountryId = user.CountryId;

                    return View(viewModel);
                }

                // Set captain flag
                foreach (var player in viewModel.Players)
                {
                    player.IsCaptain = player.Id == viewModel.CaptainId;
                }

                // Update country with team data
                country.ManagerName = viewModel.ManagerName;
                country.Players = viewModel.Players;
                country.CaptainId = viewModel.CaptainId;
                country.IsTeamComplete = true;

                // Calculate country rating
                country.Rating = await _countryService.CalculateRatingAsync(country);

                // Update country in firestore
                await _countryService.UpdateCountryAsync(user.CountryId, country);

                TempData["SuccessMessage"] = "Team created successfully!";
                return RedirectToAction("MyTeam");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                ModelState.AddModelError("", "An error occurred while creating your team. Please try again.");
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlayers([FromBody] GeneratePlayersRequest request)
        {
            try
            {
                var required = new Dictionary<string, int> { { "GK", 2 }, { "DF", 8 }, { "MD", 8 }, { "AT", 5 } };
                var counts = new Dictionary<string, int> { { "GK", 0 }, { "DF", 0 }, { "MD", 0 }, { "AT", 0 } };

                foreach (var p in request.Players)
                {
                    if (!string.IsNullOrEmpty(p.NaturalPosition) && counts.ContainsKey(p.NaturalPosition))
                    {
                        counts[p.NaturalPosition]++;
                    }
                }

                // Validate player positions do not exceed required distribution
                foreach (var kv in required)
                {
                    if (counts[kv.Key] > kv.Value)
                    {
                        return Json(new { success = false, message = $"Too many {kv.Key} already present." });
                    }
                }

                var currentTotal = request.Players.Count;

                if (currentTotal > 23)
                {
                    return Json(new { success = false, message = "You already have more than 23 players." });
                }

                // Build positions needed to reach exact distribution and 23 players
                var positionsNeeded = new List<string>();
                foreach (var kv in required)
                {
                    var need = kv.Value - counts[kv.Key];
                    if (need > 0)
                    {
                        for (int i = 0; i < need; i++) positionsNeeded.Add(kv.Key);
                    }
                }

                // If current + positionsNeeded exceeds 23, trim from the end (removes add row)
                if (currentTotal + positionsNeeded.Count > 23)
                {
                    positionsNeeded = positionsNeeded.Take(23 - currentTotal).ToList();
                }

                // Generate players for needed positions
                var generated = await _playerGeneratorService.GeneratePlayersForPositionsAsync(positionsNeeded);

                return Json(new { success = true, players = generated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating players");
                return Json(new { success = false, message = "An error occurred while generating players." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinTournament(string tournamentId)
        {
            // TODO: Register country for tournament
            return RedirectToAction("Dashboard");
        }

        // Get current user's country
        private async Task<(User? user, Country? country, string? errorMessage)> GetCurrentUserCountryAsync()
        {
            // Get user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return (null, null, null);
            }

            // Get user 
            var user = await _firebaseService.GetDocumentAsync<User>("users", userId);

            if (user == null || string.IsNullOrEmpty(user.CountryId))
            {
                return (null, null, "Country not found.");
            }

            // Get country
            var country = await _countryService.GetCountryByIdAsync(user.CountryId);

            if (country == null)
            {
                return (user, null, "Country not found.");
            }

            return (user, country, null);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//