using FreelanceMarketplace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FreelanceMarketplace.Data
{
    // Notice we inherit from IdentityDbContext<ApplicationUser> instead of just DbContext
    // This gives us all the built-in Identity tables (Users, Roles, Claims) combined with our custom fields.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define our tables
        public DbSet<Category> Categories { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProjectAttachment> ProjectAttachments { get; set; }

        // Configure the database relationships and constraints using Fluent API
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // This line is absolutely mandatory when using IdentityDbContext
            base.OnModelCreating(builder);

            // Ensure Category Names are completely unique in the database
            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Project -> Client Relationship
            // Restrict delete: Cannot delete a client if they have posted projects
            builder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(u => u.ClientProjects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> Awarded Freelancer Relationship
            // Restrict delete: Cannot delete a freelancer if they are awarded a project
            builder.Entity<Project>()
                .HasOne(p => p.AwardedFreelancer)
                .WithMany(u => u.AwardedProjects)
                .HasForeignKey(p => p.AwardedFreelancerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project -> Category Relationship
            // Restrict delete: Cannot delete a category if it has projects attached to it
            builder.Entity<Project>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bid -> Freelancer Relationship
            // Restrict delete: Cannot delete a freelancer if they have bids placed
            builder.Entity<Bid>()
                .HasOne(b => b.Freelancer)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bid -> Project Relationship
            // Cascade delete: If a project is completely deleted, delete all bids associated with it
            builder.Entity<Bid>()
                .HasOne(b => b.Project)
                .WithMany(p => p.Bids)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review -> Reviewer (Client) Relationship
            builder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Reviewee (Freelancer) Relationship
            builder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User Relationship
            // Cascade delete: If a user account is removed, wipe their notifications
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProjectAttachment -> Project Relationship
            // Cascade delete: If project is deleted, delete its file attachment records
            builder.Entity<ProjectAttachment>()
                .HasOne(pa => pa.Project)
                .WithMany(p => p.ProjectAttachments)
                .HasForeignKey(pa => pa.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}