using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Notice the '?' below. This makes it optional in the database!
        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(300)]
        public string? Skills { get; set; }

        [StringLength(200)]
        public string? ProfilePicture { get; set; }

        public decimal HourlyRate { get; set; } = 0;

        [StringLength(200)]
        public string? PortfolioUrl { get; set; }

        public int YearsExperience { get; set; } = 0;

        [StringLength(100)]
        public string? CompanyName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        // Navigation Properties initialized to prevent null warnings
        public virtual ICollection<Project> ClientProjects { get; set; } = new List<Project>();
        public virtual ICollection<Project> AwardedProjects { get; set; } = new List<Project>();
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}