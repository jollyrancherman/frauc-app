using MediatR;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Domain.Items.DTOs;

namespace Marketplace.Domain.Items.Commands;

public record CreateItemCommand(
    ItemId ItemId,
    string Title,
    string Description,
    CategoryId CategoryId,
    UserId SellerId,
    ItemCondition Condition
) : IRequest<ItemDto>;