using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services;
using FreelanceMarketplace.ViewModels.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ProjectController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: MyProjects (Client Dashboard)
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var projects = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Bids)
                .Where(p => p.ClientId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        // GET: Project/Create
        [HttpGet]
        [Authorize(Roles = "Client")]
        public IActionResult Create()
        {
            var model = new CreateProjectViewModel
            {
                CategoryList = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };
            return View(model);
        }

        // POST: Project/Create
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = new Project
                {
                    Title = model.Title,
                    Description = model.Description,
                    Budget = model.Budget,
                    MaxBudget = model.MaxBudget,
                    Deadline = model.Deadline,
                    RequiredSkills = model.RequiredSkills,
                    CategoryId = model.CategoryId,
                    ClientId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Status = ProjectStatus.Open,
                    CreatedAt = DateTime.Now
                };

                _context.Projects.Add(project);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Project posted successfully!";
                    return RedirectToAction(nameof(MyProjects));
                }
                catch (DbUpdateException ex)
                {
                    var message = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError(string.Empty, "Database error: " + message);
                }
            }

            model.CategoryList = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(model);
        }

        // GET: Browse (Public project listing)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? keyword, int? categoryId,
            decimal? minBudget, decimal? maxBudget,
            string? sortBy = "newest", int page = 1)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Include(p => p.Bids)
                .Where(p => p.Status == ProjectStatus.Open);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(kw) || p.Description.ToLower().Contains(kw));
            }
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (minBudget.HasValue)
                query = query.Where(p => p.Budget >= minBudget.Value);
            if (maxBudget.HasValue)
                query = query.Where(p => p.Budget <= maxBudget.Value);

            query = sortBy switch
            {
                "budget_high" => query.OrderByDescending(p => p.Budget),
                "budget_low" => query.OrderBy(p => p.Budget),
                "deadline" => query.OrderBy(p => p.Deadline),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            int pageSize = 9;
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var projects = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ProjectBrowseViewModel
            {
                Projects = projects,
                SearchKeyword = keyword,
                SelectedCategoryId = categoryId,
                MinBudget = minBudget,
                MaxBudget = maxBudget,
                SortBy = sortBy,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId)
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(viewModel);
        }

        // GET: SearchAjax (AJAX endpoint for browse page)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchAjax(string? keyword, int? categoryId,
            decimal? minBudget, decimal? maxBudget,
            string? sortBy = "newest", int page = 1)
        {
            var query = _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Where(p => p.Status == ProjectStatus.Open);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(kw) || p.Description.ToLower().Contains(kw));
            }
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);
            if (minBudget.HasValue)
                query = query.Where(p => p.Budget >= minBudget.Value);
            if (maxBudget.HasValue)
                query = query.Where(p => p.Budget <= maxBudget.Value);

            query = sortBy switch
            {
                "budget_high" => query.OrderByDescending(p => p.Budget),
                "budget_low" => query.OrderBy(p => p.Budget),
                "deadline" => query.OrderBy(p => p.Deadline),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            int pageSize = 9;
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var projects = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = projects.Select(p => new
            {
                id = p.Id,
                title = p.Title,
                description = p.Description.Length > 150 ? p.Description[..150] + "..." : p.Description,
                budget = p.Budget,
                maxBudget = p.MaxBudget,
                deadline = p.Deadline.ToString("yyyy-MM-dd"),
                categoryName = p.Category?.Name ?? "Uncategorized",
                categoryIcon = p.Category?.IconClass ?? "bi-folder",
                clientName = p.Client?.FullName ?? "Unknown",
                bidCount = _context.Bids.Count(b => b.ProjectId == p.Id),
                daysRemaining = (p.Deadline - DateTime.Now).Days,
                status = p.Status.ToString()
            }).ToList();

            return Json(new { projects = result, totalCount, totalPages, currentPage = page });
        }

        // GET: Project/Details/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Include(p => p.Bids)
                    .ThenInclude(b => b.Freelancer)
                .Include(p => p.AwardedFreelancer)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            project.ViewCount++;
            await _context.SaveChangesAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Bids = project.Bids.OrderByDescending(b => b.CreatedAt).ToList(),
                IsClient = project.ClientId == currentUserId,
                IsFreelancer = User.IsInRole("Freelancer"),
                HasAlreadyBid = project.Bids.Any(b => b.FreelancerId == currentUserId),
                CurrentUserBid = project.Bids.FirstOrDefault(b => b.FreelancerId == currentUserId)
            };

            return View(viewModel);
        }

        // GET: Project/Edit/5
        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (project.ClientId != currentUserId) return Forbid();
            if (project.Status != ProjectStatus.Open)
            {
                TempData["ErrorMessage"] = "Cannot edit a project that is not open.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var model = new ProjectEditViewModel
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Budget = project.Budget,
                MaxBudget = project.MaxBudget,
                Deadline = project.Deadline,
                CategoryId = project.CategoryId,
                RequiredSkills = project.RequiredSkills,
                CategoryList = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == project.CategoryId
                }).ToList()
            };

            return View(model);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Edit(ProjectEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = await _context.Projects.FindAsync(model.Id);
                if (project == null) return NotFound();

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (project.ClientId != currentUserId) return Forbid();

                project.Title = model.Title;
                project.Description = model.Description;
                project.Budget = model.Budget;
                project.MaxBudget = model.MaxBudget;
                project.Deadline = model.Deadline;
                project.CategoryId = model.CategoryId;
                project.RequiredSkills = model.RequiredSkills;
                project.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Project updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            model.CategoryList = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == model.CategoryId
            }).ToList();

            return View(model);
        }

        // POST: Project/AwardAjax
        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AwardAjax(int bidId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bid = await _context.Bids
                    .Include(b => b.Project)
                    .Include(b => b.Freelancer)
                    .FirstOrDefaultAsync(b => b.Id == bidId);

                if (bid == null)
                    return Json(new { success = false, message = "Bid not found." });

                if (bid.Project.ClientId != currentUserId)
                    return Json(new { success = false, message = "You do not own this project." });

                if (bid.Project.Status != ProjectStatus.Open)
                    return Json(new { success = false, message = "Project is no longer accepting bids." });

                bid.Status = BidStatus.Accepted;
                bid.Project.Status = ProjectStatus.InProgress;
                bid.Project.AwardedFreelancerId = bid.FreelancerId;
                bid.Project.UpdatedAt = DateTime.Now;

                var otherBids = await _context.Bids
                    .Where(b => b.ProjectId == bid.ProjectId && b.Id != bidId)
                    .ToListAsync();
                foreach (var other in otherBids)
                {
                    other.Status = BidStatus.Rejected;
                }

                await _context.SaveChangesAsync();

                await _notificationService.CreateAsync(
                    bid.FreelancerId,
                    $"Your bid on '{bid.Project.Title}' was accepted!",
                    $"/Project/Details/{bid.ProjectId}",
                    "BidAccepted");

                foreach (var other in otherBids.Where(b => b.FreelancerId != null))
                {
                    await _notificationService.CreateAsync(
                        other.FreelancerId,
                        $"Your bid on '{bid.Project.Title}' was not selected.",
                        $"/Project/Details/{bid.ProjectId}",
                        "BidRejected");
                }

                return Json(new { success = true, message = "Project awarded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error awarding project: " + ex.Message });
            }
        }

        // POST: Project/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Cancel(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (project.ClientId != currentUserId) return Forbid();

            if (project.Status != ProjectStatus.Open)
            {
                TempData["ErrorMessage"] = "Only open projects can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            project.Status = ProjectStatus.Cancelled;
            project.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project cancelled successfully.";
            return RedirectToAction(nameof(MyProjects));
        }

        // POST: Project/MarkCompleted/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (project.ClientId != currentUserId) return Forbid();

            if (project.Status != ProjectStatus.InProgress)
            {
                TempData["ErrorMessage"] = "Only in-progress projects can be marked completed.";
                return RedirectToAction(nameof(Details), new { id });
            }

            project.Status = ProjectStatus.Completed;
            project.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project marked as completed!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}