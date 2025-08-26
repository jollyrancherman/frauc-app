using MediatR;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Domain.Items.Events;

public record ItemCreatedEvent(
    ItemId ItemId,
    UserId SellerId,
    CategoryId CategoryId,
    string Title,
    DateTime CreatedAt
) : INotification;