using FluentValidation;
using Marketplace.Application.Items.Commands;
using Marketplace.Domain.Items;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Items.Validators;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    private readonly IItemRepository _itemRepository;

    public CreateItemCommandValidator(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(3, 200)
            .WithMessage("Title must be between 3 and 200 characters")
            .Must(BeValidTitle)
            .WithMessage("Title contains invalid characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.ItemId)
            .NotNull()
            .WithMessage("ItemId is required")
            .MustAsync(async (itemId, cancellation) => 
                !await ItemExistsAsync(itemId, cancellation))
            .WithMessage("Item with this ID already exists");

        RuleFor(x => x.CategoryId)
            .NotNull()
            .WithMessage("CategoryId is required");

        RuleFor(x => x.SellerId)
            .NotNull()
            .WithMessage("SellerId is required");

        RuleFor(x => x.Condition)
            .IsInEnum()
            .WithMessage("Invalid item condition");
    }

    private static bool BeValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        // Allow letters, digits, spaces, and common punctuation
        return title.All(c => char.IsLetterOrDigit(c) || 
                            char.IsWhiteSpace(c) || 
                            "'-.,!?()[]{}:;\"".Contains(c));
    }

    private async Task<bool> ItemExistsAsync(Marketplace.Domain.Items.ValueObjects.ItemId itemId, CancellationToken cancellation)
    {
        var existingItem = await _itemRepository.GetByIdAsync(itemId, cancellation);
        return existingItem != null;
    }
}