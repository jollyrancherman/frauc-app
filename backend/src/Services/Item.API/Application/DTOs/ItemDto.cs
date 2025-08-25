using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Item.API.Application.DTOs;

public class ItemDto
{
    public ItemId ItemId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CategoryId CategoryId { get; init; }
    public UserId SellerId { get; init; }
    public ItemCondition Condition { get; init; }
    public DateTime CreatedAt { get; init; }
    public IEnumerable<ItemImageDto> Images { get; init; } = Enumerable.Empty<ItemImageDto>();

    public static ItemDto FromDomain(Marketplace.Domain.Items.Item item)
    {
        return new ItemDto
        {
            ItemId = item.Id,
            Title = item.Title,
            Description = item.Description,
            CategoryId = item.CategoryId,
            SellerId = item.SellerId,
            Condition = item.Condition,
            CreatedAt = item.CreatedAt,
            Images = item.Images.Select(ItemImageDto.FromDomain)
        };
    }
}