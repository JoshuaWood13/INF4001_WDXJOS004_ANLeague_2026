using INF4001_WDXJOS004_ANLeague_2026.Services.Auth;

namespace INF4001_WDXJOS004_ANLeague_2026.Middleware
{
    public class FirebaseAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAuthenticationMiddleware> _logger;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public FirebaseAuthenticationMiddleware(RequestDelegate next, ILogger<FirebaseAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Middleware to authenticate user from Firebase token in cookie
        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var token = context.Request.Cookies["FirebaseToken"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Verify token and get ClaimsPrincipal
                    var principal = await authService.GetClaimsPrincipalAsync(token);

                    if (principal != null)
                    {
                        // Attach ClaimsPrincipal to HttpContext for authorization
                        context.User = principal;
                        _logger.LogDebug("User succesfully authenticated from token");
                    }
                    else
                    {
                        _logger.LogWarning("Expired or invalid token");

                        // Clear cookie
                        context.Response.Cookies.Delete("FirebaseToken");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing token");
                    context.Response.Cookies.Delete("FirebaseToken");
                }
            }

            await _next(context);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }

    // Extension
    //------------------------------------------------------------------------------------------------------------------------------------------//
    // Add middlleware to pipeline
    public static class FirebaseAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirebaseAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirebaseAuthenticationMiddleware>();
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------//
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//