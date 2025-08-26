using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;

namespace Marketplace.Domain.Listings.Events;

public record ListingExpiredEvent(
    ListingId ListingId
) : IDomainEvent;