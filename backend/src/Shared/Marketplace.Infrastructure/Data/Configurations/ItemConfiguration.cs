using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        // Primary key
        builder.HasKey(i => i.Id);
        
        // Value object conversions
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new ItemId(value))
            .ValueGeneratedNever();

        builder.Property(i => i.SellerId)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(i => i.CategoryId)
            .HasConversion(
                id => id.Value,
                value => new CategoryId(value))
            .IsRequired();

        // Properties
        builder.Property(i => i.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(i => i.Condition)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        // Owned entities for Images
        builder.OwnsMany(i => i.Images, imageBuilder =>
        {
            imageBuilder.ToTable("ItemImages");
            
            imageBuilder.WithOwner().HasForeignKey("ItemId");
            
            imageBuilder.HasKey("ItemId", "Id");
            
            imageBuilder.Property(img => img.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ImageId(value))
                .HasColumnName("Id");
            
            imageBuilder.Property(img => img.Url)
                .HasMaxLength(500)
                .IsRequired();
            
            imageBuilder.Property(img => img.AltText)
                .HasMaxLength(200)
                .IsRequired(false);
            
            imageBuilder.Property(img => img.IsPrimary)
                .IsRequired();
            
            imageBuilder.Property(img => img.DisplayOrder)
                .IsRequired();
        });

        // Indexes for performance
        builder.HasIndex(i => i.SellerId)
            .HasDatabaseName("IX_Items_SellerId");

        builder.HasIndex(i => i.CategoryId)
            .HasDatabaseName("IX_Items_CategoryId");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("IX_Items_CreatedAt");

        builder.HasIndex(i => new { i.CategoryId, i.CreatedAt })
            .HasDatabaseName("IX_Items_CategoryId_CreatedAt");

        builder.HasIndex(i => new { i.SellerId, i.CreatedAt })
            .HasDatabaseName("IX_Items_SellerId_CreatedAt");
    }
}