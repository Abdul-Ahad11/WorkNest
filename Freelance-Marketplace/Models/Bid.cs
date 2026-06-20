using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceMarketplace.Models
{
    public enum BidStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }

    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(1000)]
        public string ProposalMessage { get; set; }

        [Required]
        public int DeliveryDays { get; set; }

        public BidStatus Status { get; set; } = BidStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string FreelancerId { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("FreelancerId")]
        public virtual ApplicationUser Freelancer { get; set; }
    }
}