using INF4001_WDXJOS004_ANLeague_2026.Middleware;
using INF4001_WDXJOS004_ANLeague_2026.Services.AICommentary;
using INF4001_WDXJOS004_ANLeague_2026.Services.Auth;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Email;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using INF4001_WDXJOS004_ANLeague_2026.Services.Match;
using INF4001_WDXJOS004_ANLeague_2026.Services.PlayerGenerator;
using INF4001_WDXJOS004_ANLeague_2026.Services.Tournament;

using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace INF4001_WDXJOS004_ANLeague_2026
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Firebase Admin SDK
            var credentialsPath = builder.Configuration["Firebase:CredentialsPath"];
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), credentialsPath);

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(fullPath),
                ProjectId = builder.Configuration["Firebase:ProjectId"]
            });

            // Initialize Firestore
            var firestoreDb = FirestoreDb.Create(builder.Configuration["Firebase:ProjectId"]);
            builder.Services.AddSingleton(firestoreDb);

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

            // Add session support
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add authentication
            builder.Services.AddAuthentication("FirebaseAuth")
                .AddCookie("FirebaseAuth", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

            builder.Services.AddAuthorization();

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
            app.UseSession();

            // Add custom Firebase authentication middleware
            app.UseFirebaseAuthentication();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Tournament}/{action=Tournament}/{id?}");

            app.Run();
        }
    }
}
