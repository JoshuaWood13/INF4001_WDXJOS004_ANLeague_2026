using Microsoft.AspNetCore.Builder;

namespace INF4001_WDXJOS004_ANLeague_2026.Middleware
{
    public static class FirebaseAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirebaseAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirebaseAuthenticationMiddleware>();
        }
    }
}
