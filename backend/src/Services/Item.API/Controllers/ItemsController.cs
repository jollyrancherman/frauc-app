using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.Queries;
using Marketplace.Application.Items.DTOs;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Item.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IMediator mediator, ILogger<ItemsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    /// <param name="request">Create item request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created item</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ItemDto>> CreateItem(
        CreateItemRequest request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating item with title: {Title}", request.Title);

        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            request.Title,
            request.Description,
            new CategoryId(request.CategoryId),
            new UserId(request.SellerId),
            request.Condition
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to create item: {Error}", result.ErrorMessage);
            
            if (result.ErrorMessage?.Contains("already exists") == true)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Item already exists",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Failed to create item",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Successfully created item with ID: {ItemId}", result.Value!.ItemId);

        return CreatedAtAction(
            nameof(GetItem), 
            new { id = result.Value.ItemId.Value }, 
            result.Value);
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Item details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ItemDto>> GetItem(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving item with ID: {ItemId}", id);

        var query = new GetItemByIdQuery(new ItemId(id));
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Item not found: {ItemId}", id);
            return NotFound(new ProblemDetails
            {
                Title = "Item not found",
                Detail = $"Item with ID '{id}' was not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get items by seller
    /// </summary>
    /// <param name="sellerId">Seller ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of items</returns>
    [HttpGet("seller/{sellerId:guid}")]
    [ProducesResponseType(typeof(PaginatedResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ItemDto>>> GetItemsBySeller(
        Guid sellerId,
        [FromQuery][Range(1, 1000, ErrorMessage = "Page number must be between 1 and 1000")] int pageNumber = 1,
        [FromQuery][Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving items for seller: {SellerId}, Page: {PageNumber}, Size: {PageSize}", 
            sellerId, pageNumber, pageSize);

        var query = new GetItemsBySellerQuery(new UserId(sellerId), pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Failed to retrieve items",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get items by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of items</returns>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PaginatedResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ItemDto>>> GetItemsByCategory(
        Guid categoryId,
        [FromQuery][Range(1, 1000, ErrorMessage = "Page number must be between 1 and 1000")] int pageNumber = 1,
        [FromQuery][Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving items for category: {CategoryId}, Page: {PageNumber}, Size: {PageSize}", 
            categoryId, pageNumber, pageSize);

        var query = new GetItemsByCategoryQuery(new CategoryId(categoryId), pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Failed to retrieve items",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Add an image to an item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="imageFile">Image file to upload</param>
    /// <param name="isPrimary">Whether this should be the primary image</param>
    /// <param name="altText">Optional alt text for the image</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created image details</returns>
    [HttpPost("{itemId:guid}/images")]
    [ProducesResponseType(typeof(ItemImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ItemImageDto>> AddItemImage(
        Guid itemId,
        IFormFile imageFile,
        [FromForm] bool isPrimary = false,
        [FromForm] string? altText = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding image to item {ItemId} by user {UserId}", 
            itemId, GetCurrentUserId().Value);

        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid file",
                Detail = "No file was provided or file is empty",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var command = new AddItemImageCommand(
            new ItemId(itemId),
            GetCurrentUserId(),
            imageFile,
            isPrimary,
            altText);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to add image to item {ItemId}: {Error}", itemId, result.ErrorMessage);

            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Item not found",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (result.ErrorMessage?.Contains("not authorized") == true)
            {
                return Forbid();
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Failed to add image",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Successfully added image {ImageId} to item {ItemId}", 
            result.Value!.Id.Value, itemId);

        return CreatedAtAction(
            nameof(GetItem), 
            new { id = itemId }, 
            result.Value);
    }

    /// <summary>
    /// Remove an image from an item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="imageId">Image ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{itemId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemoveItemImage(
        Guid itemId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing image {ImageId} from item {ItemId} by user {UserId}", 
            imageId, itemId, GetCurrentUserId().Value);

        var command = new RemoveItemImageCommand(
            new ItemId(itemId),
            GetCurrentUserId(),
            new ImageId(imageId));

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to remove image {ImageId} from item {ItemId}: {Error}", 
                imageId, itemId, result.ErrorMessage);

            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Item or image not found",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (result.ErrorMessage?.Contains("not authorized") == true)
            {
                return Forbid();
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Failed to remove image",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Successfully removed image {ImageId} from item {ItemId}", imageId, itemId);

        return NoContent();
    }

    /// <summary>
    /// Set an image as the primary image for an item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="imageId">Image ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{itemId:guid}/images/{imageId:guid}/set-primary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetPrimaryImage(
        Guid itemId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting image {ImageId} as primary for item {ItemId} by user {UserId}", 
            imageId, itemId, GetCurrentUserId().Value);

        var command = new SetPrimaryImageCommand(
            new ItemId(itemId),
            GetCurrentUserId(),
            new ImageId(imageId));

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to set primary image {ImageId} for item {ItemId}: {Error}", 
                imageId, itemId, result.ErrorMessage);

            if (result.ErrorMessage?.Contains("not found") == true)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Item or image not found",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (result.ErrorMessage?.Contains("not authorized") == true)
            {
                return Forbid();
            }

            return BadRequest(new ProblemDetails
            {
                Title = "Failed to set primary image",
                Detail = result.ErrorMessage,
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Successfully set image {ImageId} as primary for item {ItemId}", imageId, itemId);

        return Ok(new { Success = true });
    }

    /// <summary>
    /// Get current user ID from JWT claims
    /// </summary>
    /// <returns>User ID from claims</returns>
    private UserId GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format in token");
        }

        return new UserId(userId);
    }
}

/// <summary>
/// Request model for creating an item
/// </summary>
public record CreateItemRequest(
    string Title,
    string Description,
    Guid CategoryId,
    Guid SellerId,
    ItemCondition Condition
);

