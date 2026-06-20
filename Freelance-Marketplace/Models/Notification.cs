using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceMarketplace.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string Link { get; set; }

        [StringLength(50)]
        public string Type { get; set; }

        // Foreign Key
        [Required]
        public string UserId { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}