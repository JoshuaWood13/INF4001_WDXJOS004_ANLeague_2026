using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Email
{
    public interface IEmailService
    {
        Task SendMatchResultNotificationAsync(Match match, string homeCountryEmail, string awayCountryEmail);
    }
}
