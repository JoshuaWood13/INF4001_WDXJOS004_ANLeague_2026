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
                return RedirectToAction("Tournament", "Tournament");
            }

            return View();
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Tournament", "Tournament");
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
            catch (ApplicationException appEx)
            {
                TempData["FirebaseError"] = appEx.Message;
                return View("SignUp", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user sign up");
                TempData["FirebaseError"] = "An unexpected error occurred during sign up. Please try again.";
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
                    return Json(new { success = false, message = "Authentication token is missing. Please try logging in again." });
                }

                // Verify firebase ID token
                var isValid = await _authService.VerifyTokenAsync(model.IdToken);

                if (!isValid)
                {
                    return Json(new { success = false, message = "Your session has expired. Please log in again." });
                }

                // Get user info from token
                var userId = await _authService.GetUserIdAsync(model.IdToken);
                var role = await _authService.GetUserRoleAsync(model.IdToken);

                if (userId == null || role == null)
                {
                    return Json(new { success = false, message = "Unable to retrieve your account information. Please try logging in again." });
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
                    redirectUrl = Url.Action("Tournament", "Tournament") ?? "/Tournament/Tournament";
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
                            redirectUrl = Url.Action("Tournament", "Tournament") ?? "/Tournament/Tournament";
                        }
                    }
                    else
                    {
                        // No country found
                        redirectUrl = Url.Action("Tournament", "Tournament") ?? "/";
                    }
                }

                return Json(new { success = true, redirectUrl });
            }
            catch (FirebaseAuthException fbEx)
            {
                _logger.LogError(fbEx, "Firebase authentication error during login callback");
                return Json(new { success = false, message = "Authentication failed. Please check your credentials and try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login callback");
                return Json(new { success = false, message = "An unexpected error occurred. Please try again in a moment." });
            }
        }

        // Logout user
        [HttpPost]
        [Authorize]
        public IActionResult Logout()
        {
            // Clear authentication cookie
            Response.Cookies.Delete("FirebaseToken");

            return RedirectToAction("Tournament", "Tournament");
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