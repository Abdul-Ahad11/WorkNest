using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceMarketplace.Models
{
    public class ProjectAttachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; }

        [Required]
        [StringLength(300)]
        public string FilePath { get; set; }

        [Required]
        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Foreign Key
        [Required]
        public int ProjectId { get; set; }

        // Navigation Property
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
    }
}