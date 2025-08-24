using FluentAssertions;
using Marketplace.Domain.Users.ValueObjects;

namespace User.API.Tests.Domain.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("testuser")]
    [InlineData("user123")]
    [InlineData("test_user")]
    [InlineData("test-user")]
    [InlineData("abc")]
    [InlineData("verylongusernamethatisexactlytwentyfivecharacters")] // 50 chars
    public void Username_ShouldBeCreated_WithValidUsername(string validUsername)
    {
        // Act
        var username = Username.Create(validUsername);

        // Assert
        username.Should().NotBeNull();
        username.Value.Should().Be(validUsername);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")] // Too short (minimum 3 chars)
    [InlineData("verylongusernamethatiswaytoolongandexceedsthelimitofcharacters")] // Too long (over 50 chars)
    [InlineData("user@name")] // Invalid character
    [InlineData("user name")] // Space not allowed
    [InlineData("user#name")] // Invalid character
    public void Username_ShouldThrowException_WithInvalidUsername(string? invalidUsername)
    {
        // Act & Assert
        var action = () => Username.Create(invalidUsername!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*username*");
    }

    [Fact]
    public void Username_ShouldBeEqual_WhenValuesAreSame()
    {
        // Arrange
        var username1 = Username.Create("testuser");
        var username2 = Username.Create("testuser");

        // Act & Assert
        username1.Should().Be(username2);
        (username1 == username2).Should().BeTrue();
        (username1 != username2).Should().BeFalse();
        username1.GetHashCode().Should().Be(username2.GetHashCode());
    }

    [Fact]
    public void Username_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var username1 = Username.Create("testuser1");
        var username2 = Username.Create("testuser2");

        // Act & Assert
        username1.Should().NotBe(username2);
        (username1 == username2).Should().BeFalse();
        (username1 != username2).Should().BeTrue();
    }

    [Fact]
    public void Username_ShouldConvertToString_Implicitly()
    {
        // Arrange
        var usernameValue = "testuser";
        var username = Username.Create(usernameValue);

        // Act
        string result = username;

        // Assert
        result.Should().Be(usernameValue);
    }

    [Fact]
    public void Username_ShouldBeCaseInsensitive_ForComparison()
    {
        // Arrange
        var username1 = Username.Create("TestUser");
        var username2 = Username.Create("testuser");

        // Act & Assert
        username1.Should().Be(username2);
        (username1 == username2).Should().BeTrue();
    }
}