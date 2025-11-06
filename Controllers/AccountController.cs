using FirebaseAdmin.Auth;
using INF4001_WDXJOS004_ANLeague_2026.Data;
using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;
using INF4001_WDXJOS004_ANLeague_2026.Models.Enums;
using INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels;
using INF4001_WDXJOS004_ANLeague_2026.Services.Auth;
using INF4001_WDXJOS004_ANLeague_2026.Services.Country;
using INF4001_WDXJOS004_ANLeague_2026.Services.Firebase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INF4001_WDXJOS004_ANLeague_2026.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IFirebaseService _firebaseService;
        private readonly IAuthService _authService;
        private readonly ICountryService _countryService;

        // Constructor
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public AccountController(ILogger<AccountController> logger, IFirebaseService firebaseService, IAuthService authService, ICountryService countryService)
        {
            _logger = logger;
            _firebaseService = firebaseService;
            _authService = authService;
            _countryService = countryService;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Views
        //------------------------------------------------------------------------------------------------------------------------------------------//
        public IActionResult SignUp()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//

        // Methods
        //------------------------------------------------------------------------------------------------------------------------------------------//
        // Create a new country representative account
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUpUser(SignUpViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("SignUp", viewModel);
            }

            try
            {
                // Check if country already has a representative
                var isCountryRegistered = await _countryService.CheckIfRegisteredAsync(viewModel.CountryName);

                if (isCountryRegistered)
                {
                    ModelState.AddModelError("CountryName", "This country already has a representative.");
                    return View("SignUp", viewModel);
                }

                // Create user with representative role
                var userId = await _authService.CreateUserAsync(viewModel.Email, viewModel.Password, UserRole.Representative);

                // Create initial country
                var countryId = await _countryService.CreateInitialCountryAsync(viewModel.CountryName, userId);

                // Create user document in firestore
                var user = new User
                {
                    Id = userId,
                    Email = viewModel.Email,
                    Role = UserRole.Representative.ToString(),
                    CountryId = countryId, 
                    CreatedAt = DateTime.UtcNow
                };

                await _firebaseService.AddDocumentWithIdAsync("users", userId, user);

                TempData["SuccessMessage"] = "Sign up successful!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user sign up");
                ModelState.AddModelError("", "An error occurred during sign up. Please try again.");
                return View("SignUp", viewModel);
            }
        }

        // Handles login callback from by verifying firebase ID token and setting auth cookie
        [HttpPost]
        public async Task<IActionResult> LoginCallback([FromBody] LoginCallbackModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.IdToken))
                {
                    return Json(new { success = false, message = "Invalid token" });
                }

                // Verify firebase ID token
                var isValid = await _authService.VerifyTokenAsync(model.IdToken);

                if (!isValid)
                {
                    return Json(new { success = false, message = "Invalid or expired token" });
                }

                // Get user info from token
                var userId = await _authService.GetUserIdAsync(model.IdToken);
                var role = await _authService.GetUserRoleAsync(model.IdToken);

                if (userId == null || role == null)
                {
                    return Json(new { success = false, message = "Unable to retrieve user information" });
                }

                // Set secure authentication cookie
                Response.Cookies.Append("FirebaseToken", model.IdToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,   //false, // true 
                    SameSite = SameSiteMode.Lax,  //Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                // Determine redirect URL based on role
                string redirectUrl;
                if (role == UserRole.Administrator)
                {
                    redirectUrl = Url.Action("Dashboard", "Admin") ?? "/Admin/Dashboard";
                }
                else
                {
                    // Check if representative has completed team setup
                    var user = await _firebaseService.GetDocumentAsync<User>("users", userId);

                    if (user != null && !string.IsNullOrEmpty(user.CountryId))
                    {
                        var country = await _firebaseService.GetDocumentAsync<Country>("countries", user.CountryId);

                        if (country != null && !country.IsTeamComplete)
                        {
                            // Team setup not complete
                            redirectUrl = Url.Action("MyTeam", "Representative") ?? "/Representative/MyTeam";
                        }
                        else
                        {
                            // Team is complete
                            redirectUrl = Url.Action("Dashboard", "Representative") ?? "/Representative/Dashboard";
                        }
                    }
                    else
                    {
                        // No country found
                        redirectUrl = Url.Action("Index", "Home") ?? "/";
                    }
                }

                return Json(new { success = true, redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login callback");
                return Json(new { success = false, message = "Login failed. Please try again." });
            }
        }

        // Logout user
        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            // Clear authentication cookie
            Response.Cookies.Delete("FirebaseToken");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RegisterCountry()
        {
            // TODO: Return RegisterCountryViewModel with country list
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCountry1(/* RegisterCountryViewModel model */)
        {
            // TODO: Implement country registration logic
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlayers()
        {
            // TODO: Generate 23 random players, return JSON
            return Json(new { success = false, message = "Not implemented" });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------//
    }

    // Helper model for login callback
    public class LoginCallbackModel
    {
        public string IdToken { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//