using FluentAssertions;
using Marketplace.Domain.Users.ValueObjects;

namespace User.API.Tests.Domain.ValueObjects;

public class UserIdTests
{
    [Fact]
    public void UserId_ShouldBeCreated_WithNewGuid()
    {
        // Act
        var userId = UserId.New();

        // Assert
        userId.Should().NotBeNull();
        userId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void UserId_ShouldBeCreated_WithExistingGuid()
    {
        // Arrange
        var guidValue = Guid.NewGuid();

        // Act
        var userId = UserId.Create(guidValue);

        // Assert
        userId.Should().NotBeNull();
        userId.Value.Should().Be(guidValue);
    }

    [Fact]
    public void UserId_ShouldThrowException_WithEmptyGuid()
    {
        // Act & Assert
        var action = () => UserId.Create(Guid.Empty);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*userId*");
    }

    [Fact]
    public void UserId_ShouldBeEqual_WhenValuesAreSame()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var userId1 = UserId.Create(guidValue);
        var userId2 = UserId.Create(guidValue);

        // Act & Assert
        userId1.Should().Be(userId2);
        (userId1 == userId2).Should().BeTrue();
        (userId1 != userId2).Should().BeFalse();
        userId1.GetHashCode().Should().Be(userId2.GetHashCode());
    }

    [Fact]
    public void UserId_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var userId1 = UserId.New();
        var userId2 = UserId.New();

        // Act & Assert
        userId1.Should().NotBe(userId2);
        (userId1 == userId2).Should().BeFalse();
        (userId1 != userId2).Should().BeTrue();
    }

    [Fact]
    public void UserId_ShouldConvertToGuid_Implicitly()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var userId = UserId.Create(guidValue);

        // Act
        Guid result = userId;

        // Assert
        result.Should().Be(guidValue);
    }

    [Fact]
    public void UserId_ShouldConvertToString_WithCorrectFormat()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var userId = UserId.Create(guidValue);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(guidValue.ToString());
    }
}