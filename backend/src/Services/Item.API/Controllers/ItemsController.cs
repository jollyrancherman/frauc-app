using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ItemDto>>> GetItemsBySeller(
        Guid sellerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ItemDto>>> GetItemsByCategory(
        Guid categoryId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
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

