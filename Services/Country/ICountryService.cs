using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using CountryEntity = INF4001_WDXJOS004_ANLeague_2026.Models.Entities.Country;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Country
{
    public interface ICountryService
    {
        Task<bool> CheckIfRegisteredAsync(string countryName);
        Task<string> CreateInitialCountryAsync(string countryName, string representativeId); 
        Task<string> CreateCountryAsync(CountryEntity country);
        Task<CountryEntity?> GetCountryByIdAsync(string countryId);
        Task<CountryEntity?> GetCountryByRepresentativeIdAsync(string representativeId);
        Task<(CountryEntity? entity, string documentId)> GetCountryWithIdByRepresentativeIdAsync(string representativeId);
        Task<double> CalculateRatingAsync(CountryEntity country);
        Task UpdateTournamentRegistrationAsync(string countryId, bool isRegistered);
        Task UpdateCountryAsync(string countryId, CountryEntity country);
        Task UpdateTeamStatisticsAsync(string countryId, bool won, bool drew, bool tournamentWon); 
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//