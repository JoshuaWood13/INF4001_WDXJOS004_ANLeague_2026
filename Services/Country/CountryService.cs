using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Country
{
    public class CountryService : ICountryService
    {
        // TODO: Implement country management operations
        public Task<string> CreateCountryAsync(Country country)
        {
            throw new NotImplementedException();
        }

        public Task<Country?> GetCountryByIdAsync(string countryId)
        {
            throw new NotImplementedException();
        }

        public Task<Country?> GetCountryByRepresentativeIdAsync(string representativeId)
        {
            throw new NotImplementedException();
        }

        public Task<double> CalculateAverageRatingAsync(Country country)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon)
        {
            throw new NotImplementedException();
        }
    }
}
