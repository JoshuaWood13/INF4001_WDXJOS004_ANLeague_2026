using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Match
{
    public interface IMatchService
    {
        IAsyncEnumerable<CommentaryMoment> SimulateMatchAsync(string homeCountryId, string awayCountryId);
        IAsyncEnumerable<CommentaryMoment> PlayMatchAsync(string homeCountryId, string awayCountryId);
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//