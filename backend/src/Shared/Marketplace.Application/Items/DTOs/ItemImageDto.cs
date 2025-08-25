using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.DTOs;

public record ItemImageDto(
    ImageId Id,
    string Url,
    bool IsPrimary
)
{
    public static ItemImageDto FromDomain(ItemImage itemImage) => new(
        itemImage.Id,
        itemImage.Url,
        itemImage.IsPrimary
    );
}