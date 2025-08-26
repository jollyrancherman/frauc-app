using Marketplace.Application.Common;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Listings.Services;

public interface IListingDomainService
{
    Task<Result<ListingDto>> CreateFreeListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        ValueObjects.Location location,
        CategoryId categoryId,
        CancellationToken cancellationToken = default);

    Task<Result<ListingDto>> CreateFreeToAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        ValueObjects.Location location,
        CategoryId categoryId,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    Task<Result<ListingDto>> CreateForwardAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        ValueObjects.Location location,
        CategoryId categoryId,
        Money startingPrice,
        Money? reservePrice,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    Task<Result<ListingDto>> CreateReverseAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        ValueObjects.Location location,
        CategoryId categoryId,
        Money maxPrice,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    Task<Result<ListingDto>> CreateFixedPriceListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        ValueObjects.Location location,
        CategoryId categoryId,
        Money price,
        CancellationToken cancellationToken = default);
}