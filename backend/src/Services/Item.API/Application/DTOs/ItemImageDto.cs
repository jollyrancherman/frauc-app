using Marketplace.Domain.Items.ValueObjects;

namespace Item.API.Application.DTOs;

public class ItemImageDto
{
    public ImageId Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }

    public static ItemImageDto FromDomain(ItemImage itemImage)
    {
        return new ItemImageDto
        {
            Id = itemImage.Id,
            Url = itemImage.Url,
            IsPrimary = itemImage.IsPrimary
        };
    }
}