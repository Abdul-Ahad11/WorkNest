using FreelanceMarketplace.Data;
using FreelanceMarketplace.Models;
using FreelanceMarketplace.Services;
using FreelanceMarketplace.ViewModels.Bid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public BidController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> SubmitAjax([FromBody] BidSubmitViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("; ", errors) });
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var project = await _context.Projects.FindAsync(vm.ProjectId);
                if (project == null)
                    return Json(new { success = false, message = "Project not found." });

                if (project.Status != ProjectStatus.Open)
                    return Json(new { success = false, message = "This project is no longer accepting bids." });

                bool alreadyBid = await _context.Bids.AnyAsync(b =>
                    b.ProjectId == vm.ProjectId && b.FreelancerId == currentUserId);
                if (alreadyBid)
                    return Json(new { success = false, message = "You have already bid on this project." });

                var bid = new Bid
                {
                    Amount = vm.Amount,
                    ProposalMessage = vm.ProposalMessage,
                    DeliveryDays = vm.DeliveryDays,
                    ProjectId = vm.ProjectId,
                    FreelancerId = currentUserId,
                    Status = BidStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.Bids.Add(bid);
                await _context.SaveChangesAsync();

                await _notificationService.CreateAsync(
                    project.ClientId,
                    $"New bid received on your project '{project.Title}'!",
                    $"/Project/Details/{project.Id}",
                    "NewBid");

                return Json(new { success = true, message = "Bid submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting bid: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> MyBids()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bids = await _context.Bids
                .Include(b => b.Project)
                    .ThenInclude(p => p.Category)
                .Include(b => b.Project)
                    .ThenInclude(p => p.Client)
                .Where(b => b.FreelancerId == currentUserId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bids);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> WithdrawBid(int bidId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bid = await _context.Bids
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == bidId && b.FreelancerId == currentUserId);

            if (bid == null)
            {
                TempData["ErrorMessage"] = "Bid not found.";
                return RedirectToAction(nameof(MyBids));
            }

            if (bid.Status != BidStatus.Pending)
            {
                TempData["ErrorMessage"] = "Cannot withdraw a bid that has been accepted or rejected.";
                return RedirectToAction(nameof(MyBids));
            }

            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bid withdrawn successfully.";
            return RedirectToAction(nameof(MyBids));
        }
    }
}