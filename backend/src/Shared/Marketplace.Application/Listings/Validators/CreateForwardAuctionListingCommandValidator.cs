using FluentValidation;
using Marketplace.Application.Listings.Commands;

namespace Marketplace.Application.Listings.Validators;

public class CreateForwardAuctionListingCommandValidator : AbstractValidator<CreateForwardAuctionListingCommand>
{
    public CreateForwardAuctionListingCommandValidator()
    {
        Include(new BaseListingCommandValidator());

        RuleFor(x => x.StartingPrice)
            .NotNull()
            .WithMessage("Starting price is required");

        RuleFor(x => x.StartingPrice.Amount)
            .GreaterThan(0)
            .WithMessage("Starting price must be greater than 0")
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Starting price cannot exceed $1,000,000");

        RuleFor(x => x.ReservePrice.Amount)
            .GreaterThanOrEqualTo(x => x.StartingPrice.Amount)
            .WithMessage("Reserve price must be greater than or equal to starting price")
            .When(x => x.ReservePrice != null);

        RuleFor(x => x.Duration)
            .GreaterThanOrEqualTo(TimeSpan.FromHours(1))
            .WithMessage("Auction duration must be at least 1 hour")
            .LessThanOrEqualTo(TimeSpan.FromDays(30))
            .WithMessage("Auction duration cannot exceed 30 days");
    }
}

public class BaseListingCommandValidator : AbstractValidator<object>
{
    public BaseListingCommandValidator()
    {
        // Common validation rules would go here if we had a base command
    }
}