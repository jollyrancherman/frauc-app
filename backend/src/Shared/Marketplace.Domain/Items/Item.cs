using Marketplace.Domain.Common;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Items.Events;
using Marketplace.Domain.Categories;

namespace Marketplace.Domain.Items;

public class Item : Entity<ItemId>
{
    private readonly List<ItemImage> _images = new();
    private const int MaxTitleLength = 200;
    private const int MaxImages = 10;

    public string Title { get; private set; }
    public string Description { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public UserId SellerId { get; private set; }
    public ItemCondition Condition { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<ItemImage> Images => _images.AsReadOnly();

    private Item() : base(new ItemId(Guid.NewGuid())) { } // EF Core

    private Item(
        ItemId id,
        string title,
        string description,
        CategoryId categoryId,
        UserId sellerId,
        ItemCondition condition) : base(id ?? throw new ArgumentNullException(nameof(id)))
    {
        SetTitle(title);
        Description = description ?? string.Empty;
        CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));
        SellerId = sellerId ?? throw new ArgumentNullException(nameof(sellerId));
        Condition = condition;
        CreatedAt = DateTime.UtcNow;
    }

    public static Item Create(
        ItemId id,
        string title,
        string description,
        CategoryId categoryId,
        UserId sellerId,
        ItemCondition condition)
    {
        var item = new Item(id, title, description, categoryId, sellerId, condition);
        
        // Raise domain event
        item.AddDomainEvent(new ItemCreatedEvent(id, sellerId, categoryId, title, item.CreatedAt));
        
        return item;
    }

    public void UpdateDetails(string title, string description)
    {
        SetTitle(title);
        Description = description ?? string.Empty;
    }

    public void AddImage(ItemImage image)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (_images.Count >= MaxImages)
            throw new InvalidOperationException($"Cannot add more than {MaxImages} images");

        // If this is the first image, make it primary
        if (!_images.Any())
        {
            image.SetPrimary(true);
        }
        // If this image is marked as primary, unset other primary images
        else if (image.IsPrimary)
        {
            foreach (var existingImage in _images)
            {
                existingImage.SetPrimary(false);
            }
        }

        _images.Add(image);
    }

    public void RemoveImage(ImageId imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            _images.Remove(image);

            // If we removed the primary image and there are other images, make the first one primary
            if (image.IsPrimary && _images.Any())
            {
                _images.First().SetPrimary(true);
            }
        }
    }

    public void SetPrimaryImage(ImageId imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new InvalidOperationException($"Image with id {imageId} not found");

        // Unset all primary flags
        foreach (var img in _images)
        {
            img.SetPrimary(false);
        }

        // Set the new primary
        image.SetPrimary(true);
    }

    public void ChangeCondition(ItemCondition condition)
    {
        Condition = condition;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters", nameof(title));

        Title = title;
    }
}