using FluentAssertions;
using Xunit;
using FluentValidation.TestHelper;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.Validators;
using Marketplace.Application.Common.Security;
using NSubstitute;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Listing.Application.Tests.Validators;

public class CreateFreeListingCommandValidatorTests
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly CreateFreeListingCommandValidator _validator;

    public CreateFreeListingCommandValidatorTests()
    {
        _htmlSanitizer = Substitute.For<IHtmlSanitizer>();
        _validator = new CreateFreeListingCommandValidator(_htmlSanitizer);
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Valid Title",
            Description: "Valid Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(command.Title).Returns(false);
        _htmlSanitizer.Sanitize(command.Description).Returns(command.Description);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Title_Contains_Html()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "<script>alert('xss')</script>",
            Description: "Valid Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(command.Title).Returns(true);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot contain HTML or script tags");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Should_Fail_When_Title_Is_Empty(string title)
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: title,
            Description: "Valid Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Should_Fail_When_Title_Exceeds_200_Characters()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: longTitle,
            Description: "Valid Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(longTitle).Returns(false);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Should_Sanitize_Description_Html()
    {
        // Arrange
        var htmlDescription = "<p>Description with <script>alert('xss')</script> HTML</p>";
        var sanitizedDescription = "<p>Description with  HTML</p>";
        
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Valid Title",
            Description: htmlDescription,
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(command.Title).Returns(false);
        _htmlSanitizer.Sanitize(htmlDescription).Returns(sanitizedDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
        _htmlSanitizer.Received(1).Sanitize(htmlDescription);
    }

    [Fact]
    public void Should_Fail_When_Location_Latitude_Invalid()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Valid Title",
            Description: "Valid Description",
            Location: new Location(91, 0), // Invalid latitude
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(command.Title).Returns(false);
        _htmlSanitizer.Sanitize(command.Description).Returns(command.Description);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location.Latitude)
            .WithErrorMessage("Latitude must be between -90 and 90");
    }

    [Fact]
    public void Should_Fail_When_Location_Longitude_Invalid()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Valid Title",
            Description: "Valid Description",
            Location: new Location(0, 181), // Invalid longitude
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _htmlSanitizer.ContainsHtml(command.Title).Returns(false);
        _htmlSanitizer.Sanitize(command.Description).Returns(command.Description);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location.Longitude)
            .WithErrorMessage("Longitude must be between -180 and 180");
    }
}