using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FreelanceMarketplace.ViewModels.Project
{
    public class ProjectEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000)]
        public decimal Budget { get; set; }

        [Range(1, 1000000)]
        public decimal? MaxBudget { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(300)]
        public string? RequiredSkills { get; set; }

        public IEnumerable<SelectListItem>? CategoryList { get; set; }
    }
}