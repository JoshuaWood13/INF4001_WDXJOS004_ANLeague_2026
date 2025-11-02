namespace INF4001_WDXJOS004_ANLeague_2026.Middleware
{
    public class FirebaseAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirebaseAuthenticationMiddleware> _logger;

        public FirebaseAuthenticationMiddleware(RequestDelegate next, ILogger<FirebaseAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // TODO: Implement Firebase authentication middleware
            // Extract Firebase token from cookie
            // Verify token using Firebase Admin SDK
            // Extract user claims (uid, email, role)
            // Create ClaimsPrincipal and attach to HttpContext.User
            // Enable [Authorize] attributes with role-based access

            await _next(context);
        }
    }
}
