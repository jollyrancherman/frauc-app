using MediatR;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Common;

namespace Marketplace.Application.Items.Commands;

public record CreateItemCommand(
    ItemId ItemId,
    string Title,
    string Description,
    CategoryId CategoryId,
    UserId SellerId,
    ItemCondition Condition
) : IRequest<Result<ItemDto>>;