using FluentValidation;
using Item.API.Application.Commands;

namespace Item.API.Application.Commands;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.ItemId)
            .NotNull()
            .WithMessage("ItemId is required");

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
}