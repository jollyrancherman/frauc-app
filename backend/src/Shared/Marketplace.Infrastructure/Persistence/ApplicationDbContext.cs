using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Items;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Persistence.Configurations;

namespace Marketplace.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Listing> Listings { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Enable PostGIS extension
        modelBuilder.HasPostgresExtension("postgis");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // NetTopologySuite configuration is done via UseNpgsql().UseNetTopologySuite() in startup
    }
}