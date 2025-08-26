using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
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

        // String properties
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

        // Map domain SpatialPoint to PostGIS Point
        builder.Property<NetTopologySuite.Geometries.Point>("LocationPoint")
            .HasColumnType("geometry (point, 4326)")
            .HasColumnName("LocationPoint")
            .HasConversion(
                spatialPoint => spatialPoint,
                point => point,
                new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueComparer<NetTopologySuite.Geometries.Point>(
                    (l, r) => l != null && r != null && l.Equals(r),
                    v => v.GetHashCode(),
                    v => (NetTopologySuite.Geometries.Point)v.Copy()));

        // Create spatial index for performance
        builder.HasIndex("LocationPoint")
            .HasMethod("GIST")
            .HasDatabaseName("IX_Listings_LocationPoint_Spatial");

        // Configure domain SpatialPoint as owned entity
        builder.OwnsOne(l => l.SpatialPoint, spatialBuilder =>
        {
            spatialBuilder.Ignore(sp => sp.SpatialPoint); // Prevent recursion
        });

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

        // Auction settings as complex type
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

            auctionBuilder.OwnsOne(a => a.MaxPrice, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("AuctionMaxPriceAmount")
                    .HasPrecision(18, 2);
                
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("AuctionMaxPriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            auctionBuilder.OwnsOne(a => a.BuyNowPrice, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("AuctionBuyNowPriceAmount")
                    .HasPrecision(18, 2);
                
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("AuctionBuyNowPriceCurrency")
                    .HasMaxLength(3)
                    .HasDefaultValue("USD");
            });

            auctionBuilder.OwnsOne(a => a.MinimumBidIncrement, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("AuctionMinBidIncrementAmount")
                    .HasPrecision(18, 2);
                
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("AuctionMinBidIncrementCurrency")
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

        builder.Property(l => l.ExpiresAt)
            .IsRequired(false);

        builder.Property(l => l.CompletedAt)
            .IsRequired(false);

        builder.Property(l => l.DeletedAt)
            .IsRequired(false);

        // Primitive properties
        builder.Property(l => l.ViewCount)
            .HasDefaultValue(0);

        // Computed property for soft delete
        builder.Ignore(l => l.IsDeleted);

        // Optimistic concurrency control
        builder.Property(l => l.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes for performance
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

        builder.HasIndex(l => l.ExpiresAt)
            .HasDatabaseName("IX_Listings_ExpiresAt")
            .HasFilter("\"ExpiresAt\" IS NOT NULL");

        builder.HasIndex(l => l.DeletedAt)
            .HasDatabaseName("IX_Listings_DeletedAt")
            .HasFilter("\"DeletedAt\" IS NOT NULL");

        // Composite indexes for common queries
        builder.HasIndex(l => new { l.Status, l.CategoryId, l.CreatedAt })
            .HasDatabaseName("IX_Listings_Status_Category_Created");

        builder.HasIndex(l => new { l.SellerId, l.Status, l.CreatedAt })
            .HasDatabaseName("IX_Listings_Seller_Status_Created");

        // Global query filter for soft deletes (optional - can be applied at query level instead)
        // builder.HasQueryFilter(l => l.DeletedAt == null);
    }
}