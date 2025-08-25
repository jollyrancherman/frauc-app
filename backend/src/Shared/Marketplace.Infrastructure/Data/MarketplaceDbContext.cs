using Marketplace.Domain.Users;
using Marketplace.Domain.Items;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Infrastructure.Data;

public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}