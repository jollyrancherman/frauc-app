using FluentAssertions;
using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using User.API.IntegrationTests.Infrastructure;

namespace User.API.IntegrationTests.Repositories;

public class UserRepositoryTests : IntegrationTestBase
{
    private IUserRepository _userRepository = null!;

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<IUserRepository, UserRepository>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _userRepository = ServiceProvider.GetRequiredService<IUserRepository>();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserToDatabase()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = await ExecuteInTransactionAsync(async context =>
        {
            var addedUser = await _userRepository.AddAsync(user);
            await context.SaveChangesAsync();
            return addedUser;
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);

        // Verify in database
        var persistedUser = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        persistedUser.Should().NotBeNull();
        persistedUser!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentId = UserId.New();

        // Act
        var result = await _userRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        var result = await _userRepository.GetByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentEmail = Email.Create("nonexistent@example.com");

        // Act
        var result = await _userRepository.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        var result = await _userRepository.GetByUsernameAsync(user.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var nonExistentUsername = Username.Create("nonexistent");

        // Act
        var result = await _userRepository.GetByUsernameAsync(nonExistentUsername);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        var result = await _userRepository.ExistsWithEmailAsync(user.Email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithEmailAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var nonExistentEmail = Email.Create("nonexistent@example.com");

        // Act
        var result = await _userRepository.ExistsWithEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsWithUsernameAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        var result = await _userRepository.ExistsWithUsernameAsync(user.Username);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsWithUsernameAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var nonExistentUsername = Username.Create("nonexistent");

        // Act
        var result = await _userRepository.ExistsWithUsernameAsync(nonExistentUsername);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            existingUser!.UpdateProfile("Jane", "Smith");
            await _userRepository.UpdateAsync(existingUser);
            await context.SaveChangesAsync();
        });

        // Assert
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("Jane");
        updatedUser.LastName.Should().Be("Smith");
        updatedUser.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = CreateTestUser();
        await SeedUser(user);

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            await _userRepository.DeleteAsync(existingUser!);
            await context.SaveChangesAsync();
        });

        // Assert
        var deletedUser = await _userRepository.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    private static Marketplace.Domain.Users.User CreateTestUser()
    {
        var userId = UserId.New();
        var email = Email.Create($"test{Guid.NewGuid():N}@example.com");
        var username = Username.Create($"testuser{Guid.NewGuid():N}");
        return Marketplace.Domain.Users.User.Create(userId, email, username, "John", "Doe");
    }

    private async Task SeedUser(Marketplace.Domain.Users.User user)
    {
        await ExecuteInTransactionAsync(async context =>
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        });

        // Clear domain events after seeding
        user.ClearDomainEvents();
    }
}