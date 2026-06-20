using FreelanceMarketplace.Models;
using System.Collections.Generic;

namespace FreelanceMarketplace.ViewModels.Project
{
    public class ProjectDetailsViewModel
    {
        public Models.Project Project { get; set; }
        public List<Models.Bid> Bids { get; set; } = new();
        public Models.Bid? CurrentUserBid { get; set; }
        public bool IsClient { get; set; }
        public bool IsFreelancer { get; set; }
        public bool HasAlreadyBid { get; set; }
        public double AverageClientRating { get; set; }
    }
}