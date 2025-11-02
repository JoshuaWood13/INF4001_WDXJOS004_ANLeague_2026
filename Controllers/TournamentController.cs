using Microsoft.AspNetCore.Mvc;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ILogger<TournamentController> _logger;

        public TournamentController(ILogger<TournamentController> logger)
        {
            _logger = logger;
        }

        // GET: Tournament/MatchDetails/{matchId}
        public async Task<IActionResult> MatchDetails(string matchId)
        {
            // TODO: Get match details, return MatchDetailViewModel
            // If "Played": Show full commentary timeline
            // If "Simulated": Show final score and goal scorers
            return View();
        }
    }
}
