using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FDP.Models;

namespace FDP.Data
{
    public class ApplicationDbContext : DbContext  // Updated inheritance
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<EvaluationType> EvaluationType { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure Identity models are configured properly
            base.OnModelCreating(modelBuilder);
           

            // UserRole Configuration
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserID, ur.RoleID });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID);

            // Feedback Configuration
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.EvaluationType)
                .WithMany()
                .HasForeignKey(f => f.EvalTypeID);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.EnteredByUser)
                .WithMany()
                .HasForeignKey(f => f.EnteredByUserID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
