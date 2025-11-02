using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class TeamDetailsViewModel
    {
        public Country Country { get; set; } = new Country();
        public string CaptainName { get; set; } = string.Empty;
        public double WinLossPercentage { get; set; }
    }
}
