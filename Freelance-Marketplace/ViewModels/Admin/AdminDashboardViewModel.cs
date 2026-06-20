using FreelanceMarketplace.Models;
using System.Collections.Generic;

namespace FreelanceMarketplace.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalFreelancers { get; set; }
        public int TotalProjects { get; set; }
        public int OpenProjects { get; set; }
        public int InProgressProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int CancelledProjects { get; set; }
        public int TotalBids { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<string, int> ProjectsPerCategory { get; set; } = new();
        public Dictionary<string, int> RegistrationsLast30Days { get; set; } = new();
        public List<Models.Project> RecentProjects { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();
    }
}