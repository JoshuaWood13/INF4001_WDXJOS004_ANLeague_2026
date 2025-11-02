using INF4001_WDXJOS004_ANLeague_2026.Middleware;
using INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary;
using INF4001_WDXJOS004_ANLeague_2026.Services.Auth;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Email;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using INF4001_WDXJOS004_ANLeague_2026.Services.Match;
using INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator;
using INF4001_WDXJOS004_ANLeague_2026.Services.Tournament;

namespace INF4001_WDXJOS004_ANLeague_2026
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register all services with dependency injection
            builder.Services.AddScoped<IFirebaseService, FirebaseService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPlayerGeneratorService, PlayerGeneratorService>();
            builder.Services.AddScoped<ICountryService, CountryService>();
            builder.Services.AddScoped<ITournamentService, TournamentService>();
            builder.Services.AddScoped<IMatchService, MatchService>();
            builder.Services.AddScoped<IAICommentaryService, AICommentaryService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            // TODO: Configure Firebase Admin SDK
            // TODO: Configure authentication middleware

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add custom Firebase authentication middleware
            app.UseFirebaseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
