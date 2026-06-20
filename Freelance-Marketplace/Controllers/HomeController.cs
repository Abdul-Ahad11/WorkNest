using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProjects = await _context.Projects.CountAsync();
            ViewBag.TotalFreelancers = (await _userManager.GetUsersInRoleAsync("Freelancer")).Count;
            ViewBag.CompletedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            ViewBag.TotalCategories = categories.Count;

            var recentProjects = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.Client)
                .Where(p => p.Status == ProjectStatus.Open)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();
            ViewBag.RecentProjects = recentProjects;

            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}