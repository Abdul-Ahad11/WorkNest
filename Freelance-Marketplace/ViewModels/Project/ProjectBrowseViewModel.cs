using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace FreelanceMarketplace.ViewModels.Project
{
    public class ProjectBrowseViewModel
    {
        public List<Models.Project> Projects { get; set; } = new();
        public SelectList? Categories { get; set; }
        public string? SearchKeyword { get; set; }
        public int? SelectedCategoryId { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public string? SortBy { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
    }
}