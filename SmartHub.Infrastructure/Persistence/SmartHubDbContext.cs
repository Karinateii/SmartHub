using Microsoft.EntityFrameworkCore;
using SmartHub.Domain.Entities;

namespace SmartHub.Infrastructure.Persistence
{
    public class SmartHubDbContext : DbContext
    {
        public SmartHubDbContext(DbContextOptions<SmartHubDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration Classes Automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartHubDbContext).Assembly);
        }
    }
}

/* This registers the User entity, Autoloads configurations and sets up EF */