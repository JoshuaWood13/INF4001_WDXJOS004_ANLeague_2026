using System.ComponentModel.DataAnnotations;
using INF4001_WDXJOS004_ANLeague_2026.Models.Entities;

namespace INF4001_WDXJOS004_ANLeague_2026.Models.ViewModels
{
    public class RegisterCountryViewModel
    {
        [Required(ErrorMessage = "Please select a country")]
        [Display(Name = "Country")]
        public string CountryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manager name is required")]
        [Display(Name = "Manager Name")]
        public string ManagerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a captain")]
        [Display(Name = "Captain")]
        public string CaptainId { get; set; } = string.Empty;

        public List<Player> Players { get; set; } = new List<Player>();
    }
}
