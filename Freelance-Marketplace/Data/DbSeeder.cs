using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FreelanceMarketplace.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Seed Roles
                string[] roles = { "Admin", "Client", "Freelancer" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // 2. Seed Admin User
                if (await userManager.FindByEmailAsync("admin@freelancehub.com") == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = "admin@freelancehub.com",
                        Email = "admin@freelancehub.com",
                        FullName = "System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    var result = await userManager.CreateAsync(admin, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, "Admin");
                    }
                }

                // 3. Seed Sample Clients
                var clientData = new[]
                {
                    new { Email = "client1@test.com", Name = "Ahmed Khan", Company = "TechCorp" },
                    new { Email = "client2@test.com", Name = "Sara Malik", Company = "DesignStudio" },
                    new { Email = "client3@test.com", Name = "Ali Hassan", Company = "StartupHub" }
                };

                foreach (var c in clientData)
                {
                    if (await userManager.FindByEmailAsync(c.Email) == null)
                    {
                        var client = new ApplicationUser
                        {
                            UserName = c.Email,
                            Email = c.Email,
                            FullName = c.Name,
                            CompanyName = c.Company,
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        var result = await userManager.CreateAsync(client, "Client@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(client, "Client");
                        }
                    }
                }

                // 4. Seed Sample Freelancers
                var freelancerData = new[]
                {
                    new { Email = "freelancer1@test.com", Name = "Usman Tariq", Skills = "ASP.NET, SQL Server, JavaScript", Rate = 25m },
                    new { Email = "freelancer2@test.com", Name = "Fatima Noor", Skills = "UI/UX Design, Figma, Adobe XD", Rate = 20m },
                    new { Email = "freelancer3@test.com", Name = "Hamza Raza", Skills = "React, Node.js, MongoDB", Rate = 30m }
                };

                foreach (var f in freelancerData)
                {
                    if (await userManager.FindByEmailAsync(f.Email) == null)
                    {
                        var freelancer = new ApplicationUser
                        {
                            UserName = f.Email,
                            Email = f.Email,
                            FullName = f.Name,
                            Skills = f.Skills,
                            HourlyRate = f.Rate,
                            EmailConfirmed = true,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        var result = await userManager.CreateAsync(freelancer, "Free@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(freelancer, "Freelancer");
                        }
                    }
                }

                // 5. Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Web Development", Description = "Websites, web apps, and backend systems", IconClass = "bi-globe", CreatedAt = DateTime.Now },
                        new Category { Name = "Mobile Apps", Description = "iOS and Android mobile applications", IconClass = "bi-phone", CreatedAt = DateTime.Now },
                        new Category { Name = "Graphic Design", Description = "Logos, branding, and visual design", IconClass = "bi-palette", CreatedAt = DateTime.Now },
                        new Category { Name = "Content Writing", Description = "Articles, blogs, copywriting, and editing", IconClass = "bi-pencil", CreatedAt = DateTime.Now },
                        new Category { Name = "Digital Marketing", Description = "SEO, social media, and advertising", IconClass = "bi-megaphone", CreatedAt = DateTime.Now },
                        new Category { Name = "Data Science", Description = "Data analysis, ML, and AI solutions", IconClass = "bi-bar-chart", CreatedAt = DateTime.Now }
                    );
                    await context.SaveChangesAsync();
                }

                // 6. Seed Sample Projects
                if (!await context.Projects.AnyAsync())
                {
                    var client1 = await userManager.FindByEmailAsync("client1@test.com");
                    var client2 = await userManager.FindByEmailAsync("client2@test.com");
                    var client3 = await userManager.FindByEmailAsync("client3@test.com");

                    var webDev = await context.Categories.FirstAsync(c => c.Name == "Web Development");
                    var mobile = await context.Categories.FirstAsync(c => c.Name == "Mobile Apps");
                    var design = await context.Categories.FirstAsync(c => c.Name == "Graphic Design");
                    var writing = await context.Categories.FirstAsync(c => c.Name == "Content Writing");
                    var marketing = await context.Categories.FirstAsync(c => c.Name == "Digital Marketing");

                    context.Projects.AddRange(
                        new Project
                        {
                            Title = "E-Commerce Website with Admin Panel",
                            Description = "We need a full-featured e-commerce platform with product management, shopping cart, payment integration (Stripe/PayPal), and an admin dashboard for analytics.",
                            Budget = 5000,
                            MaxBudget = 8000,
                            Deadline = DateTime.Now.AddDays(30),
                            Status = ProjectStatus.Open,
                            RequiredSkills = "ASP.NET Core, SQL Server, React, Stripe API",
                            ClientId = client1.Id,
                            CategoryId = webDev.Id,
                            CreatedAt = DateTime.Now
                        },
                        new Project
                        {
                            Title = "Mobile Fitness Tracking App",
                            Description = "Build a cross-platform mobile app for fitness tracking with workout logging, progress charts, meal planning, and push notifications.",
                            Budget = 8000,
                            MaxBudget = 12000,
                            Deadline = DateTime.Now.AddDays(45),
                            Status = ProjectStatus.Open,
                            RequiredSkills = "Flutter, Firebase, REST APIs",
                            ClientId = client1.Id,
                            CategoryId = mobile.Id,
                            CreatedAt = DateTime.Now
                        },
                        new Project
                        {
                            Title = "Complete Brand Identity Design",
                            Description = "Looking for a creative designer to build our brand from scratch - logo, color palette, typography, business cards, and social media templates.",
                            Budget = 1500,
                            MaxBudget = 2500,
                            Deadline = DateTime.Now.AddDays(14),
                            Status = ProjectStatus.Open,
                            RequiredSkills = "Adobe Illustrator, Photoshop, Figma",
                            ClientId = client2.Id,
                            CategoryId = design.Id,
                            CreatedAt = DateTime.Now
                        },
                        new Project
                        {
                            Title = "SEO Blog Content Package (20 Articles)",
                            Description = "Need 20 SEO-optimized blog articles (1500+ words each) for our SaaS product. Topics include productivity, team management, and remote work.",
                            Budget = 2000,
                            MaxBudget = 3000,
                            Deadline = DateTime.Now.AddDays(21),
                            Status = ProjectStatus.Open,
                            RequiredSkills = "SEO writing, SaaS knowledge, Research skills",
                            ClientId = client2.Id,
                            CategoryId = writing.Id,
                            CreatedAt = DateTime.Now
                        },
                        new Project
                        {
                            Title = "Social Media Marketing Campaign",
                            Description = "Plan and execute a 3-month social media campaign across Instagram, LinkedIn, and Twitter to increase our B2B SaaS brand awareness.",
                            Budget = 3000,
                            MaxBudget = 5000,
                            Deadline = DateTime.Now.AddDays(10),
                            Status = ProjectStatus.Open,
                            RequiredSkills = "Social Media Strategy, Content Creation, Analytics",
                            ClientId = client3.Id,
                            CategoryId = marketing.Id,
                            CreatedAt = DateTime.Now
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}