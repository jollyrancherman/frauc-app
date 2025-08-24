using FluentAssertions;
using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using Marketplace.Domain.Users.Events;

namespace User.API.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var userId = UserId.New();
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = Marketplace.Domain.Users.User.Create(userId, email, username, firstName, lastName);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.Email.Should().Be(email);
        user.Username.Should().Be(username);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_ShouldRaiseDomainEvent_WhenCreated()
    {
        // Arrange
        var userId = UserId.New();
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = Marketplace.Domain.Users.User.Create(userId, email, username, firstName, lastName);

        // Assert
        var domainEvent = user.DomainEvents.Should().ContainSingle().Which;
        domainEvent.Should().BeOfType<UserCreatedEvent>();
        var userCreatedEvent = (UserCreatedEvent)domainEvent;
        userCreatedEvent.UserId.Should().Be(userId);
        userCreatedEvent.Email.Should().Be(email.Value);
        userCreatedEvent.Username.Should().Be(username.Value);
    }

    [Fact]
    public void User_ShouldUpdateProfile_WithValidData()
    {
        // Arrange
        var user = CreateTestUser();
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_ShouldRaiseDomainEvent_WhenProfileUpdated()
    {
        // Arrange
        var user = CreateTestUser();
        user.ClearDomainEvents(); // Clear creation event
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        var domainEvent = user.DomainEvents.Should().ContainSingle().Which;
        domainEvent.Should().BeOfType<UserProfileUpdatedEvent>();
        var profileUpdatedEvent = (UserProfileUpdatedEvent)domainEvent;
        profileUpdatedEvent.UserId.Should().Be(user.Id);
        profileUpdatedEvent.FirstName.Should().Be(newFirstName);
        profileUpdatedEvent.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void User_ShouldDeactivate_WhenRequested()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_ShouldActivate_WhenRequested()
    {
        // Arrange
        var user = CreateTestUser();
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_ShouldThrowException_WhenFirstNameIsInvalid(string? invalidFirstName)
    {
        // Arrange
        var userId = UserId.New();
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        var lastName = "Doe";

        // Act & Assert
        var action = () => Marketplace.Domain.Users.User.Create(userId, email, username, invalidFirstName!, lastName);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*firstName*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_ShouldThrowException_WhenLastNameIsInvalid(string? invalidLastName)
    {
        // Arrange
        var userId = UserId.New();
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        var firstName = "John";

        // Act & Assert
        var action = () => Marketplace.Domain.Users.User.Create(userId, email, username, firstName, invalidLastName!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*lastName*");
    }

    private static Marketplace.Domain.Users.User CreateTestUser()
    {
        var userId = UserId.New();
        var email = Email.Create("test@example.com");
        var username = Username.Create("testuser");
        return Marketplace.Domain.Users.User.Create(userId, email, username, "John", "Doe");
    }
}