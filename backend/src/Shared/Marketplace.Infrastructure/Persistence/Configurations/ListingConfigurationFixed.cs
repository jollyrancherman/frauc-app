using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Infrastructure.Persistence.Configurations;

public class ListingConfigurationFixed : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");

        // Primary key
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Id)
            .HasConversion(
                id => id.Value,
                value => new ListingId(value))
            .ValueGeneratedNever();

        // Value object conversions
        builder.Property(l => l.ItemId)
            .HasConversion(
                id => id.Value,
                value => new ItemId(value));

        builder.Property(l => l.SellerId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        builder.Property(l => l.CategoryId)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value));

        // String properties with validation
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(5000);

        // Location as complex type (lat/lng)
        builder.OwnsOne(l => l.Location, locationBuilder =>
        {
            locationBuilder.Property(loc => loc.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(18, 12);
            
            locationBuilder.Property(loc => loc.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(18, 12);
        });

        // Domain SpatialPoint ignored - we'll use a shadow property for PostGIS
        builder.Ignore(l => l.SpatialPoint);

        // PostGIS Point as shadow property (infrastructure concern)
        builder.Property<Point>("LocationPoint")
            .HasColumnType("geometry (point, 4326)")
            .HasColumnName("LocationPoint");

        // Create spatial index
        builder.HasIndex("LocationPoint")
            .HasMethod("GIST")
            .HasDatabaseName("IX_Listings_LocationPoint_Spatial");

        // Enum properties
        builder.Property(l => l.ListingType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Money value object
        builder.OwnsOne(l => l.CurrentPrice, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("CurrentPriceAmount")
                .HasPrecision(18, 2);
            
            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("CurrentPriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        // Auction settings as complex type (simplified)
        builder.OwnsOne(l => l.AuctionSettings, auctionBuilder =>
        {
            auctionBuilder.OwnsOne(a => a.StartingPrice, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("AuctionStartingPriceAmount")
                    .HasPrecision(18, 2);
                
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("AuctionStartingPriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            auctionBuilder.OwnsOne(a => a.ReservePrice, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("AuctionReservePriceAmount")
                    .HasPrecision(18, 2);
                
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("AuctionReservePriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            auctionBuilder.Property(a => a.Duration)
                .HasColumnName("AuctionDuration");

            auctionBuilder.Property(a => a.AllowAutoBidding)
                .HasColumnName("AuctionAllowAutoBidding");
        });

        // DateTime properties
        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(l => l.ExpiresAt);
        builder.Property(l => l.CompletedAt);
        builder.Property(l => l.DeletedAt);

        // Primitive properties
        builder.Property(l => l.ViewCount)
            .HasDefaultValue(0);

        // Computed property ignored
        builder.Ignore(l => l.IsDeleted);

        // Performance indexes
        builder.HasIndex(l => l.SellerId)
            .HasDatabaseName("IX_Listings_SellerId");

        builder.HasIndex(l => l.CategoryId)
            .HasDatabaseName("IX_Listings_CategoryId");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("IX_Listings_Status");

        builder.HasIndex(l => l.ListingType)
            .HasDatabaseName("IX_Listings_ListingType");

        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("IX_Listings_CreatedAt");

        // Composite indexes for performance
        builder.HasIndex(l => new { l.Status, l.CategoryId, l.CreatedAt })
            .HasDatabaseName("IX_Listings_Status_Category_Created");

        builder.HasIndex(l => new { l.SellerId, l.Status, l.CreatedAt })
            .HasDatabaseName("IX_Listings_Seller_Status_Created");
    }
}