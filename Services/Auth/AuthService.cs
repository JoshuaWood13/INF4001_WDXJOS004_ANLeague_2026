using FirebaseAdmin.Auth;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using System.Security.Claims;

namespace INF4001_WDXJOS004_ANLeague_2026.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;

        // Contructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Verify a Firebase ID token
        public async Task<bool> VerifyTokenAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decodedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: unable to verify firebase token ");
                return false;
            }
        }

        // Create a new Firebase user with role-based access
        public async Task<string> CreateUserAsync(string email, string password, UserRole role)
        {
            try
            {
                var userArgs = new UserRecordArgs()
                {
                    Email = email,
                    Password = password,
                    EmailVerified = false,
                    Disabled = false,
                };

                // Create user
                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);

                // Set custom claims for role-based acess
                await SetCustomClaimsAsync(userRecord.Uid, role);

                _logger.LogInformation($"User {email} succesfully with role {role}");

                return userRecord.Uid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating firebase user");
                throw new Exception("Faild to create new user");
            }
        }

        // Set custom claims role for a user
        public async Task SetCustomClaimsAsync(string userId, UserRole role)
        {
            try
            {
                var claims = new Dictionary<string, object>()
                {
                    { "role", role.ToString() }
                };

                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userId, claims);

                _logger.LogInformation($"Custom claims set for user {userId} with role {role}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting custom claims");
                throw;
            }
        }

        // Get user role from a Firebase ID token
        public Task<UserRole?> GetUserRoleAsync(string idToken)
        {
            try
            {
                var decodedToken = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                if (decodedToken.Result.Claims.TryGetValue("role", out var roleClaim))
                {
                    if (Enum.TryParse<UserRole>(roleClaim.ToString(), out var role))
                    {
                        return Task.FromResult<UserRole?>(role);
                    }
                }

                return Task.FromResult<UserRole?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role from token");
                return Task.FromResult<UserRole?>(null);
            }
        }

        // Get user ID from a Firebase ID token
        public async Task<string?> GetUserIdAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decodedToken.Uid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from token");
                return null;
            }
        }

        // Get user email from a Firebase ID token
        public async Task<string?> GetUserEmailAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decodedToken.Claims.TryGetValue("email", out var email)
                    ? email.ToString()
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user email from token");
                return null;
            }
        }

        // Create ClaimsPrincipal from Firebase ID token
        public async Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(string idToken)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                    new Claim(ClaimTypes.Email, decodedToken.Claims.ContainsKey("email")? decodedToken.Claims["email"].ToString(): "")
                };

                if (decodedToken.Claims.TryGetValue("role", out var roleClaim))
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.ToString()));
                }

                var identity = new ClaimsIdentity(claims, "FirebaseAuth");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ClaimsPrincipal from token");
                return null;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//