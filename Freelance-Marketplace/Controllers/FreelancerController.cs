using FreelanceMarketplace.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Controllers
{
    [Authorize(Roles = "Freelancer")]
    public class FreelancerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FreelancerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get projects this freelancer has bid on
            var myBids = await _context.Bids
                .Include(b => b.Project)
                .Where(b => b.FreelancerId == userId)
                .ToListAsync();

            return View(myBids);
        }
    }
}