using FluentValidation;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Common.Security;

namespace Marketplace.Application.Listings.Validators;

public class CreateFreeListingCommandValidator : AbstractValidator<CreateFreeListingCommand>
{
    private readonly IHtmlSanitizer _htmlSanitizer;

    public CreateFreeListingCommandValidator(IHtmlSanitizer htmlSanitizer)
    {
        _htmlSanitizer = htmlSanitizer;

        RuleFor(x => x.ItemId)
            .NotEmpty()
            .WithMessage("ItemId is required");

        RuleFor(x => x.SellerId)
            .NotEmpty()
            .WithMessage("SellerId is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .Must(title => !_htmlSanitizer.ContainsHtml(title))
            .WithMessage("Title cannot contain HTML or script tags");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(5000)
            .WithMessage("Description cannot exceed 5000 characters");

        RuleFor(x => x.Location)
            .NotNull()
            .WithMessage("Location is required");

        RuleFor(x => x.Location.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Location.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required");
    }

    private static bool NotContainHtml(string text)
    {
        return !text.Contains('<') && !text.Contains('>');
    }

    private static bool NotContainMaliciousContent(string text)
    {
        var prohibitedWords = new[] { "script", "javascript:", "onload", "onerror", "onclick" };
        return !prohibitedWords.Any(word => text.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}