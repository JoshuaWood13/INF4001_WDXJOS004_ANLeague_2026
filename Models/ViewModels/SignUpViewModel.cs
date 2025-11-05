using System.ComponentModel.DataAnnotations;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a country")]
        public string CountryName { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//