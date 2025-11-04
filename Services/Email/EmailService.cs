using MatchEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Match;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Email
{
    public class EmailService : IEmailService
    {
        // TODO: Implement SendGrid email notifications
        // Send match result notifications to both team representatives
        // Include final score, goal scorers, match type (played/simulated)
        // Include link to match details page
        public Task SendMatchResultNotificationAsync(MatchEntity match, string homeCountryEmail, string awayCountryEmail)
        {
            throw new NotImplementedException();
        }
    }
}
