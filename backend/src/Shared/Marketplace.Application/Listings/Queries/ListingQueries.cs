using MediatR;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Listings.Queries;

public record GetListingByIdQuery(
    ListingId ListingId
) : IRequest<Result<ListingDto>>;

public record GetListingByItemIdQuery(
    ItemId ItemId
) : IRequest<Result<ListingDto>>;

public record GetListingsBySellerQuery(
    UserId SellerId,
    int PageNumber = 1,
    int PageSize = 10,
    ListingStatus? Status = null,
    ListingType? Type = null
) : IRequest<Result<PagedResult<ListingDto>>>;

public record GetListingsByCategoryQuery(
    CategoryId CategoryId,
    int PageNumber = 1,
    int PageSize = 10,
    ListingStatus? Status = null,
    ListingType? Type = null
) : IRequest<Result<PagedResult<ListingDto>>>;

public record GetNearbyListingsQuery(
    Location Center,
    double RadiusKm,
    int PageNumber = 1,
    int PageSize = 10,
    ListingType? Type = null
) : IRequest<Result<PagedResult<ListingDto>>>;

public record SearchListingsQuery(
    string? SearchTerm,
    CategoryId? CategoryId,
    Location? Center,
    double? RadiusKm,
    Money? MinPrice,
    Money? MaxPrice,
    ListingType? Type,
    ListingStatus? Status,
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "CreatedAt",
    string SortDirection = "DESC"
) : IRequest<Result<PagedResult<ListingDto>>>;