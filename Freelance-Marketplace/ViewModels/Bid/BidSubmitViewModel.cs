using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.ViewModels.Bid
{
    public class BidSubmitViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [MinLength(20)]
        [StringLength(2000)]
        public string ProposalMessage { get; set; } = string.Empty;

        [Required]
        [Range(1, 365)]
        public int DeliveryDays { get; set; }
    }
}