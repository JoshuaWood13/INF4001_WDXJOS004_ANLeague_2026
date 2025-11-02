using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary
{
    public interface IAICommentaryService
    {
        Task<string> GenerateCommentaryAsync(string homeTeamName, string awayTeamName, int homeScore, int awayScore, int minute, CommentaryType eventType, string? playerName = null);
    }
}
