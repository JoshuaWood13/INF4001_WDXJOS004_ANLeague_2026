using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    [Authorize(Roles = "Representative")]
    public class RepresentativeController : Controller
    {
        private readonly ILogger<RepresentativeController> _logger;

        public RepresentativeController(ILogger<RepresentativeController> logger)
        {
            _logger = logger;
        }

        // GET: Representative/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // TODO: Display tournament bracket with "Join Tournament" button if eligible
            return View();
        }

        // GET: Representative/MyTeam
        public async Task<IActionResult> MyTeam()
        {
            // TODO: Display squad list, positions, ratings, team statistics
            return View();
        }

        // POST: Representative/JoinTournament
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinTournament(string tournamentId)
        {
            // TODO: Register country for tournament
            return RedirectToAction("Dashboard");
        }
    }
}
