using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services;
using FreelanceMarketplace.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotificationService _notificationService;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var clients = await _userManager.GetUsersInRoleAsync("Client");
            var freelancers = await _userManager.GetUsersInRoleAsync("Freelancer");
            var totalProjects = await _context.Projects.CountAsync();
            var openProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Open);
            var inProgressProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.InProgress);
            var completedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
            var cancelledProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Cancelled);
            var totalBids = await _context.Bids.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();

            var projectsPerCategory = await _context.Categories
                .Select(c => new { c.Name, Count = c.Projects.Count })
                .ToDictionaryAsync(k => k.Name, v => v.Count);

            var last30Days = DateTime.Now.AddDays(-30);
            var registrations = await _context.Users
                .Where(u => u.CreatedAt >= last30Days)
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key.ToString("MMM dd"), Count = g.Count() })
                .ToDictionaryAsync(k => k.Date, v => v.Count);

            var recentProjects = await _context.Projects
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalClients = clients.Count,
                TotalFreelancers = freelancers.Count,
                TotalProjects = totalProjects,
                OpenProjects = openProjects,
                InProgressProjects = inProgressProjects,
                CompletedProjects = completedProjects,
                CancelledProjects = cancelledProjects,
                TotalBids = totalBids,
                TotalReviews = totalReviews,
                ProjectsPerCategory = projectsPerCategory,
                RegistrationsLast30Days = registrations,
                RecentProjects = recentProjects,
                RecentUsers = recentUsers
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Users(string? search, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
            }

            int pageSize = 15;
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userRoles = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    isActive = user.IsActive,
                    message = user.IsActive ? "User activated." : "User deactivated."
                });
            }

            return Json(new { success = false, message = "Failed to update user status." });
        }

        public async Task<IActionResult> Projects(string? status, int page = 1)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, out var projectStatus))
            {
                query = query.Where(p => p.Status == projectStatus);
            }

            int pageSize = 15;
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.StatusFilter = status;

            return View(projects);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return Json(new { success = false, message = "Project not found." });

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Project deleted." });
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    Category = c,
                    ProjectCount = c.Projects.Count
                })
                .OrderBy(c => c.Category.Name)
                .ToListAsync();

            ViewBag.CategoryStats = categories;
            return View(await _context.Categories.OrderBy(c => c.Name).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.Categories.AnyAsync(c => c.Name == category.Name);
                if (exists)
                {
                    TempData["ErrorMessage"] = "Category with this name already exists.";
                    return RedirectToAction(nameof(Categories));
                }

                category.CreatedAt = DateTime.Now;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Category created successfully.";
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null)
                return Json(new { success = false, message = "Category not found." });

            existing.Name = category.Name;
            existing.Description = category.Description;
            existing.IconClass = category.IconClass;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Projects).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return Json(new { success = false, message = "Category not found." });

            if (category.Projects.Any())
                return Json(new { success = false, message = "Cannot delete category with active projects." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Category deleted." });
        }
    }
}