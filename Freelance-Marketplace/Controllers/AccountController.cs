using FreelanceMarketplace.Models;
using FreelanceMarketplace.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace FreelanceMarketplace.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Displays the registration form.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            // If user is already logged in, redirect them home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Processes the registration form, creates user, assigns role, and logs them in.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    // Only save company name if they are a client, only save skills if freelancer
                    CompanyName = model.Role == "Client" ? model.CompanyName : null,
                    Skills = model.Role == "Freelancer" ? model.Skills : null
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign the selected role
                    await _userManager.AddToRoleAsync(user, model.Role);

                    // Log the user in immediately
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Registration successful! Welcome to WorkNest.";

                    // Redirect to correct dashboard
                    if (model.Role == "Client") return RedirectToAction("MyProjects", "Project");
                    if (model.Role == "Freelancer") return RedirectToAction("Browse", "Project");

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Displays the login form.
        /// </summary>
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Authenticates the user and redirects them based on their assigned role.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    // Update LastLoginAt
                    user.LastLoginAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    TempData["SuccessMessage"] = "Logged in successfully.";

                    // If they were trying to access a secure page, send them back to it
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    // Otherwise, redirect based on role
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin")) return RedirectToAction("Dashboard", "Admin");
                    if (roles.Contains("Client")) return RedirectToAction("MyProjects", "Project");
                    if (roles.Contains("Freelancer")) return RedirectToAction("Browse", "Project");

                    return LocalRedirect(returnUrl ?? "/Client/Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        /// <summary>
        /// Logs the user out and clears cookies.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the logged-in user's profile data.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Bio = user.Bio,
                Skills = user.Skills,
                HourlyRate = user.HourlyRate,
                PortfolioUrl = user.PortfolioUrl,
                YearsExperience = user.YearsExperience,
                CompanyName = user.CompanyName,
                CurrentProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        /// <summary>
        /// Updates the user's profile and handles profile picture file uploads.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.Bio = model.Bio;
                user.Skills = model.Skills;
                user.HourlyRate = model.HourlyRate;
                user.PortfolioUrl = model.PortfolioUrl;
                user.YearsExperience = model.YearsExperience;
                user.CompanyName = model.CompanyName;

                // Handle Profile Picture Upload
                if (model.ProfilePictureFile != null)
                {
                    // Map physical path to wwwroot/uploads/profiles
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");

                    // Create folder if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Create a unique filename to prevent overwriting
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePictureFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePictureFile.CopyToAsync(fileStream);
                    }

                    // Update database path
                    user.ProfilePicture = "/uploads/profiles/" + uniqueFileName;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Re-populate picture if form fails validation
            model.CurrentProfilePicture = user.ProfilePicture;
            return View(model);
        }
    }
}
