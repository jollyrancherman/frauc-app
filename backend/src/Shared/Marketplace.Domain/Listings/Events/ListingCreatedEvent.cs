using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Domain.Listings.Events;

public record ListingCreatedEvent(
    ListingId ListingId,
    ItemId ItemId,
    UserId SellerId,
    ListingType ListingType
) : IDomainEvent;