using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using System.Security.Claims;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> VerifyTokenAsync(string idToken);
        Task<string> CreateUserAsync(string email, string password, UserRole role);
        Task SetCustomClaimsAsync(string userId, UserRole role);
        Task<UserRole?> GetUserRoleAsync(string idToken);
        Task<string?> GetUserIdAsync(string idToken);
        Task<string?> GetUserEmailAsync(string idToken);
        Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(string idToken);
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//