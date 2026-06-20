using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.ViewModels.Auth
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Bio { get; set; }

        [Display(Name = "Primary Skills (comma separated)")]
        public string Skills { get; set; }

        [Display(Name = "Hourly Rate ($)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Portfolio URL")]
        [Url]
        public string PortfolioUrl { get; set; }

        [Display(Name = "Years of Experience")]
        public int YearsExperience { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        public string CurrentProfilePicture { get; set; }

        [Display(Name = "Upload New Picture")]
        public IFormFile ProfilePictureFile { get; set; }
    }
}