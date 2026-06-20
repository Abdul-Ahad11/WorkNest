using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [StringLength(50)]
        public string IconClass { get; set; } = "bi-folder";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual ICollection<Project> Projects { get; set; }
    }
}