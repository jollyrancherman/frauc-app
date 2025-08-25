using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;

namespace Marketplace.Domain.Listings.Events;

public record ListingCompletedEvent(
    ListingId ListingId,
    DateTime CompletedAt
) : IDomainEvent;