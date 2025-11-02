using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Country
{
    public interface ICountryService
    {
        Task<string> CreateCountryAsync(Country country);
        Task<Country?> GetCountryByIdAsync(string countryId);
        Task<Country?> GetCountryByRepresentativeIdAsync(string representativeId);
        Task<double> CalculateAverageRatingAsync(Country country);
        Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon);
    }
}
