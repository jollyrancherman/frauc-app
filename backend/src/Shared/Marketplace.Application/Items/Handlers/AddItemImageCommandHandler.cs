using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.Handlers;

/// <summary>
/// Handler for adding images to items
/// </summary>
public class AddItemImageCommandHandler : IRequestHandler<AddItemImageCommand, Result<ItemImageDto>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddItemImageCommandHandler> _logger;

    public AddItemImageCommandHandler(
        IItemRepository itemRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<AddItemImageCommandHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ItemImageDto>> Handle(AddItemImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Adding image to item {ItemId} by user {UserId}", 
                request.ItemId.Value, request.UserId.Value);

            // Get the item
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                _logger.LogWarning("Item not found: {ItemId}", request.ItemId.Value);
                return Result.Failure<ItemImageDto>("Item not found");
            }

            // Verify ownership
            if (item.SellerId != request.UserId)
            {
                _logger.LogWarning("User {UserId} not authorized to modify item {ItemId}", 
                    request.UserId.Value, request.ItemId.Value);
                return Result.Failure<ItemImageDto>("You are not authorized to modify this item");
            }

            // Upload the image
            var uploadResult = await _fileStorageService.UploadImageAsync(
                request.ImageFile, 
                "items", 
                cancellationToken);

            if (!uploadResult.IsSuccess)
            {
                _logger.LogError("Failed to upload image: {Error}", uploadResult.ErrorMessage);
                return Result.Failure<ItemImageDto>(uploadResult.ErrorMessage!);
            }

            // Create the ItemImage value object
            var imageId = new ImageId(Guid.NewGuid());
            var itemImage = new ItemImage(
                imageId,
                uploadResult.Value!,
                request.IsPrimary,
                item.Images.Count,
                request.AltText);

            // Add image to item
            item.AddImage(itemImage);

            // Save changes
            await _itemRepository.UpdateAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added image {ImageId} to item {ItemId}", 
                imageId.Value, request.ItemId.Value);

            // Return DTO
            var dto = new ItemImageDto(
                imageId,
                uploadResult.Value!,
                request.AltText,
                itemImage.IsPrimary,
                itemImage.DisplayOrder);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image to item {ItemId}", request.ItemId.Value);
            return Result.Failure<ItemImageDto>($"Failed to add image to item: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for removing images from items
/// </summary>
public class RemoveItemImageCommandHandler : IRequestHandler<RemoveItemImageCommand, Result<bool>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveItemImageCommandHandler> _logger;

    public RemoveItemImageCommandHandler(
        IItemRepository itemRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<RemoveItemImageCommandHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> Handle(RemoveItemImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Removing image {ImageId} from item {ItemId} by user {UserId}", 
                request.ImageId.Value, request.ItemId.Value, request.UserId.Value);

            // Get the item
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                _logger.LogWarning("Item not found: {ItemId}", request.ItemId.Value);
                return Result.Failure<bool>("Item not found");
            }

            // Verify ownership
            if (item.SellerId != request.UserId)
            {
                _logger.LogWarning("User {UserId} not authorized to modify item {ItemId}", 
                    request.UserId.Value, request.ItemId.Value);
                return Result.Failure<bool>("You are not authorized to modify this item");
            }

            // Find the image
            var imageToRemove = item.Images.FirstOrDefault(img => img.Id == request.ImageId);
            if (imageToRemove == null)
            {
                _logger.LogWarning("Image not found: {ImageId}", request.ImageId.Value);
                return Result.Failure<bool>("Image not found");
            }

            // Remove from item (this also handles primary image logic)
            item.RemoveImage(request.ImageId);

            // Delete from storage (best effort - don't fail if this fails)
            var deleteResult = await _fileStorageService.DeleteImageAsync(imageToRemove.Url, cancellationToken);
            if (!deleteResult.IsSuccess)
            {
                _logger.LogWarning("Failed to delete image file: {Error}", deleteResult.ErrorMessage);
            }

            // Save changes
            await _itemRepository.UpdateAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully removed image {ImageId} from item {ItemId}", 
                request.ImageId.Value, request.ItemId.Value);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing image {ImageId} from item {ItemId}", 
                request.ImageId.Value, request.ItemId.Value);
            return Result.Failure<bool>($"Failed to remove image: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for setting primary image
/// </summary>
public class SetPrimaryImageCommandHandler : IRequestHandler<SetPrimaryImageCommand, Result<bool>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetPrimaryImageCommandHandler> _logger;

    public SetPrimaryImageCommandHandler(
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        ILogger<SetPrimaryImageCommandHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Setting image {ImageId} as primary for item {ItemId} by user {UserId}", 
                request.ImageId.Value, request.ItemId.Value, request.UserId.Value);

            // Get the item
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                _logger.LogWarning("Item not found: {ItemId}", request.ItemId.Value);
                return Result.Failure<bool>("Item not found");
            }

            // Verify ownership
            if (item.SellerId != request.UserId)
            {
                _logger.LogWarning("User {UserId} not authorized to modify item {ItemId}", 
                    request.UserId.Value, request.ItemId.Value);
                return Result.Failure<bool>("You are not authorized to modify this item");
            }

            // Set primary image
            item.SetPrimaryImage(request.ImageId);

            // Save changes
            await _itemRepository.UpdateAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully set image {ImageId} as primary for item {ItemId}", 
                request.ImageId.Value, request.ItemId.Value);

            return Result.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation setting primary image: {Message}", ex.Message);
            return Result.Failure<bool>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary image {ImageId} for item {ItemId}", 
                request.ImageId.Value, request.ItemId.Value);
            return Result.Failure<bool>($"Failed to set primary image: {ex.Message}");
        }
    }
}