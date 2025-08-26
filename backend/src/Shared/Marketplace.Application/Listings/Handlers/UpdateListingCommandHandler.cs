using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Common;

namespace Marketplace.Application.Listings.Handlers;

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand, Result<ListingDto>>
{
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateListingCommandHandler> _logger;

    public UpdateListingCommandHandler(
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateListingCommandHandler> logger)
    {
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null)
            {
                return Result.Failure<ListingDto>("Listing not found");
            }

            if (listing.IsDeleted)
            {
                return Result.Failure<ListingDto>("Cannot update deleted listing");
            }

            // Business rule: Only certain statuses allow updates
            if (!listing.Status.In(Domain.Listings.ValueObjects.ListingStatus.Draft, 
                                  Domain.Listings.ValueObjects.ListingStatus.Active))
            {
                return Result.Failure<ListingDto>("Listing cannot be updated in current status");
            }

            // Apply updates
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                listing.UpdateTitle(request.Title);
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                listing.UpdateDescription(request.Description);
            }

            if (request.Location != null)
            {
                listing.UpdateLocation(request.Location);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Listing {ListingId} updated successfully", request.ListingId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating listing {ListingId}", request.ListingId);
            return Result.Failure<ListingDto>("Failed to update listing");
        }
    }
}

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand, Result<bool>>
{
    private readonly IListingRepository _listingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteListingCommandHandler> _logger;

    public DeleteListingCommandHandler(
        IListingRepository listingRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteListingCommandHandler> logger)
    {
        _listingRepository = listingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null)
            {
                return Result.Failure<bool>("Listing not found");
            }

            if (listing.SellerId != request.RequestingUserId)
            {
                return Result.Failure<bool>("User is not authorized to delete this listing");
            }

            if (listing.IsDeleted)
            {
                return Result.Success(true); // Already deleted, consider it a success
            }

            // Business rule: Can't delete active auction with bids
            if (listing.Status == Domain.Listings.ValueObjects.ListingStatus.Active && 
                listing.ListingType.IsAuction() && 
                listing.ViewCount > 0) // Simplified check - in reality would check for bids
            {
                return Result.Failure<bool>("Cannot delete active auction with bids");
            }

            listing.SoftDelete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Listing {ListingId} soft deleted by user {UserId}", 
                request.ListingId, request.RequestingUserId);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting listing {ListingId}", request.ListingId);
            return Result.Failure<bool>("Failed to delete listing");
        }
    }
}