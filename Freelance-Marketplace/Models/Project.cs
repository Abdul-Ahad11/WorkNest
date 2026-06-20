using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceMarketplace.Models
{
    public enum ProjectStatus
    {
        Open = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Budget { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxBudget { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Open;

        [StringLength(300)]
        public string RequiredSkills { get; set; }

        [StringLength(200)]
        public string AttachmentPath { get; set; }

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [Required]
        public string ClientId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string AwardedFreelancerId { get; set; }

        // Navigation Properties
        [ForeignKey("ClientId")]
        public virtual ApplicationUser Client { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("AwardedFreelancerId")]
        public virtual ApplicationUser AwardedFreelancer { get; set; }

        public virtual ICollection<Bid> Bids { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<ProjectAttachment> ProjectAttachments { get; set; }
    }
}