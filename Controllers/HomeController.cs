using System.Diagnostics;
using INF4001_WDXJOS004_ANLeague_2026.Models;
using Microsoft.AspNetCore.Mvc;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Home/MatchHighlights
        public async Task<IActionResult> MatchHighlights()
        {
            // TODO: List all completed matches with filters
            return View();
        }

        // GET: Home/GoalScorers
        public async Task<IActionResult> GoalScorers()
        {
            // TODO: Ranking of top goal scorers
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
