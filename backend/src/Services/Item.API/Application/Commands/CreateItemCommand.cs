using MediatR;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Item.API.Application.DTOs;

namespace Item.API.Application.Commands;

public record CreateItemCommand(
    ItemId ItemId,
    string Title,
    string Description,
    CategoryId CategoryId,
    UserId SellerId,
    ItemCondition Condition
) : IRequest<ItemDto>;