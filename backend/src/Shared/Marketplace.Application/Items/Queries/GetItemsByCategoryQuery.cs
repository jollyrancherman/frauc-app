using MediatR;
using Marketplace.Application.Common;
using Marketplace.Application.Items.DTOs;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Items.Queries;

public record GetItemsByCategoryQuery(
    CategoryId CategoryId,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedResponse<ItemDto>>>;