using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Items.DTOs;

public record ItemDto(
    ItemId ItemId,
    string Title,
    string Description,
    CategoryId CategoryId,
    UserId SellerId,
    ItemCondition Condition,
    DateTime CreatedAt,
    IReadOnlyList<ItemImageDto> Images
)
{
    public static ItemDto FromDomain(Marketplace.Domain.Items.Item item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.CategoryId,
        item.SellerId,
        item.Condition,
        item.CreatedAt,
        item.Images.Select(ItemImageDto.FromDomain).ToList()
    );
}