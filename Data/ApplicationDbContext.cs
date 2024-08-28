using FDP.Models;
using Microsoft.EntityFrameworkCore;

namespace FDP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

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
            // Configure the primary key, foreign keys, and relationships
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.EvaluationType)
                .WithMany()
                .HasForeignKey(f => f.EvalTypeID);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.EnteredByUser)
                .WithMany()
                .HasForeignKey(f => f.EnteredByUserID);

            base.OnModelCreating(modelBuilder);
        }
    }
}
