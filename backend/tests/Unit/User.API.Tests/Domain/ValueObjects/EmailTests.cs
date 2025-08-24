using FluentAssertions;
using Marketplace.Domain.Users.ValueObjects;

namespace User.API.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("123@example.net")]
    public void Email_ShouldBeCreated_WithValidEmailAddress(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Email_ShouldThrowException_WithInvalidEmailAddress(string? invalidEmail)
    {
        // Act & Assert
        var action = () => Email.Create(invalidEmail!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void Email_ShouldBeEqual_WhenValuesAreSame()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
        (email1 != email2).Should().BeFalse();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Email_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 == email2).Should().BeFalse();
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void Email_ShouldConvertToString_Implicitly()
    {
        // Arrange
        var emailValue = "test@example.com";
        var email = Email.Create(emailValue);

        // Act
        string result = email;

        // Assert
        result.Should().Be(emailValue);
    }
}