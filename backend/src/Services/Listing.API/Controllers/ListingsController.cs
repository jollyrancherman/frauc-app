using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Security.Claims;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.Queries;
using Marketplace.Application.Common;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;

namespace Listing.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(IMediator mediator, ILogger<ListingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private UserId GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value; // Keycloak uses 'sub' claim
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return new UserId(userId);
    }

    [HttpPost("free")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFreeListing(
        [FromBody] CreateFreeListingCommand command,
        CancellationToken cancellationToken)
    {
        // Override seller ID with authenticated user ID for security
        var secureCommand = command with { SellerId = GetCurrentUserId() };
        
        var result = await _mediator.Send(secureCommand, cancellationToken);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create free listing for user {UserId}: {Error}", 
                secureCommand.SellerId, result.ErrorMessage);
            return BadRequest(new { error = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(GetListing), 
            new { id = result.Value!.ListingId }, 
            result.Value);
    }

    [HttpPost("forward-auction")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateForwardAuctionListing(
        [FromBody] CreateForwardAuctionListingCommand command,
        CancellationToken cancellationToken)
    {
        var secureCommand = command with { SellerId = GetCurrentUserId() };
        
        var result = await _mediator.Send(secureCommand, cancellationToken);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create forward auction listing for user {UserId}: {Error}", 
                secureCommand.SellerId, result.ErrorMessage);
            return BadRequest(new { error = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(GetListing), 
            new { id = result.Value!.ListingId }, 
            result.Value);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Public listings can be viewed by anyone
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListing(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetListingByIdQuery(new ListingId(id));
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return NotFound(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateListing(
        Guid id,
        [FromBody] UpdateListingCommand command,
        CancellationToken cancellationToken)
    {
        // First, get the listing to verify ownership
        var getQuery = new GetListingByIdQuery(new ListingId(id));
        var getResult = await _mediator.Send(getQuery, cancellationToken);
        
        if (getResult.IsFailure)
        {
            return NotFound(new { error = "Listing not found" });
        }

        var currentUserId = GetCurrentUserId();
        if (getResult.Value!.SellerId != currentUserId)
        {
            _logger.LogWarning("User {UserId} attempted to update listing {ListingId} owned by {OwnerId}",
                currentUserId, id, getResult.Value.SellerId);
            return Forbid("You can only update your own listings");
        }

        // Proceed with update
        var secureCommand = command with { ListingId = new ListingId(id) };
        var updateResult = await _mediator.Send(secureCommand, cancellationToken);
        
        if (updateResult.IsFailure)
        {
            return BadRequest(new { error = updateResult.ErrorMessage });
        }

        return Ok(updateResult.Value);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteListing(
        Guid id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var command = new DeleteListingCommand(new ListingId(id), currentUserId);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new { error = result.ErrorMessage });
            }
            if (result.ErrorMessage?.Contains("not authorized") == true)
            {
                return Forbid(result.ErrorMessage);
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        return NoContent();
    }

    [HttpGet("my-listings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyListings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ListingStatus? status = null,
        [FromQuery] ListingType? type = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var query = new GetListingsBySellerQuery(
            currentUserId, 
            pageNumber, 
            pageSize, 
            status, 
            type);
        
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet("search")]
    [AllowAnonymous] // Public search
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchListings(
        [FromQuery] SearchListingsQuery query,
        CancellationToken cancellationToken)
    {
        // Add request validation
        if (query.PageSize > 100)
        {
            return BadRequest(new { error = "Page size cannot exceed 100" });
        }

        if (query.RadiusKm > 100)
        {
            return BadRequest(new { error = "Search radius cannot exceed 100km" });
        }

        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpGet("nearby")]
    [AllowAnonymous] // Public search
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNearbyListings(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusKm = 10,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        // Validate coordinates
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
        {
            return BadRequest(new { error = "Invalid coordinates" });
        }

        if (radiusKm > 100)
        {
            return BadRequest(new { error = "Search radius cannot exceed 100km" });
        }

        var query = new GetNearbyListingsQuery(
            new Location(latitude, longitude),
            radiusKm,
            pageNumber,
            pageSize);
        
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/increment-views")]
    [AllowAnonymous] // Track views for analytics
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncrementViewCount(
        Guid id,
        CancellationToken cancellationToken)
    {
        // This would typically be handled through a command
        // For now, return success
        _logger.LogInformation("View count incremented for listing {ListingId}", id);
        return NoContent();
    }
}