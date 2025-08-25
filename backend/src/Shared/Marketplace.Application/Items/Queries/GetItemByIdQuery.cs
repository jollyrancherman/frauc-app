using MediatR;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Common;

namespace Marketplace.Application.Items.Queries;

public record GetItemByIdQuery(ItemId ItemId) : IRequest<Result<ItemDto>>;