using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasColumnName("Email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasConversion(
                username => username.Value,
                value => Username.Create(value))
            .HasColumnName("Username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("UpdatedAt");

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");

        // Ignore domain events - they should be handled separately
        builder.Ignore(u => u.DomainEvents);
    }
}