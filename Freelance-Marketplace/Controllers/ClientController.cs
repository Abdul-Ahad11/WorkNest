using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FreelanceMarketplace.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch all projects created by this specific client
            var projects = await _context.Projects
                .Include(p => p.Bids)
                .Where(p => p.ClientId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }
    }
}