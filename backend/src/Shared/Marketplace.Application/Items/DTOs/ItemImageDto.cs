using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.DTOs;

public record ItemImageDto(
    ImageId Id,
    string Url,
    string? AltText,
    bool IsPrimary,
    int DisplayOrder
)
{
    public static ItemImageDto FromDomain(ItemImage itemImage) => new(
        itemImage.Id,
        itemImage.Url,
        itemImage.AltText,
        itemImage.IsPrimary,
        itemImage.DisplayOrder
    );
}