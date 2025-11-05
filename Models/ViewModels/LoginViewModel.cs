using System.ComponentModel.DataAnnotations;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
//--------------------------------------------------------X END OF FILE X-------------------------------------------------------------------//