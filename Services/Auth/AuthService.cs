using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Auth
{
    public class AuthService : IAuthService
    {
        // TODO: Implement Firebase Authentication operations
        public Task<bool> VerifyTokenAsync(string idToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateUserAsync(string email, string password, UserRole role)
        {
            throw new NotImplementedException();
        }

        public Task SetCustomClaimsAsync(string userId, UserRole role)
        {
            throw new NotImplementedException();
        }

        public Task<UserRole?> GetUserRoleAsync(string idToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetUserIdAsync(string idToken)
        {
            throw new NotImplementedException();
        }
    }
}
