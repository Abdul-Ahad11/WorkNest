using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceMarketplace.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string ReviewerId { get; set; }

        [Required]
        public string RevieweeId { get; set; }

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser Reviewer { get; set; }

        [ForeignKey("RevieweeId")]
        public virtual ApplicationUser Reviewee { get; set; }
    }
}