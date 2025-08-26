using MediatR;
using Marketplace.Application.Common;
using Marketplace.Application.Items.DTOs;
using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.Queries;

public record GetItemsBySellerQuery(
    UserId SellerId,
    int PageNumber,
    int PageSize
) : IRequest<Result<PaginatedResponse<ItemDto>>>;

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}