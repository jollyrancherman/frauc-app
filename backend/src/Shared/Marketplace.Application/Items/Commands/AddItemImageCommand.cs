using MediatR;
using Microsoft.AspNetCore.Http;
using Marketplace.Application.Common;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Application.Items.DTOs;

namespace Marketplace.Application.Items.Commands;

/// <summary>
/// Command to add an image to an item
/// </summary>
/// <param name="ItemId">The ID of the item</param>
/// <param name="UserId">The ID of the user performing the action</param>
/// <param name="ImageFile">The image file to upload</param>
/// <param name="IsPrimary">Whether this should be the primary image</param>
/// <param name="AltText">Optional alt text for the image</param>
public record AddItemImageCommand(
    ItemId ItemId,
    UserId UserId,
    IFormFile ImageFile,
    bool IsPrimary,
    string? AltText
) : IRequest<Result<ItemImageDto>>;

/// <summary>
/// Command to remove an image from an item
/// </summary>
/// <param name="ItemId">The ID of the item</param>
/// <param name="UserId">The ID of the user performing the action</param>
/// <param name="ImageId">The ID of the image to remove</param>
public record RemoveItemImageCommand(
    ItemId ItemId,
    UserId UserId,
    ImageId ImageId
) : IRequest<Result<bool>>;

/// <summary>
/// Command to set an image as the primary image
/// </summary>
/// <param name="ItemId">The ID of the item</param>
/// <param name="UserId">The ID of the user performing the action</param>
/// <param name="ImageId">The ID of the image to set as primary</param>
public record SetPrimaryImageCommand(
    ItemId ItemId,
    UserId UserId,
    ImageId ImageId
) : IRequest<Result<bool>>;