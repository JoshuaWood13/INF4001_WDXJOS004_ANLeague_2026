using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary
{
    public class AICommentaryService : IAICommentaryService
    {
        // TODO: Implement AI commentary generation
        // Generate commentary for each match moment
        // Build appropriate prompts based on event type
        // Handle API calls to AI service (Gemini/OpenAI)
        // Provide fallback commentary if API fails
        public Task<string> GenerateCommentaryAsync(string homeTeamName, string awayTeamName, int homeScore, int awayScore, int minute, CommentaryType eventType, string? playerName = null)
        {
            throw new NotImplementedException();
        }
    }
}
