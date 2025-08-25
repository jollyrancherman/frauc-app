using Marketplace.Domain.Common;

namespace Marketplace.Domain.Items.ValueObjects;

public class ItemImage : ValueObject
{
    public ImageId Id { get; private set; }
    public string Url { get; private set; }
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    public ItemImage(ImageId id, string url, bool isPrimary, int displayOrder = 0, string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty", nameof(url));

        if (!IsValidUrl(url))
            throw new ArgumentException("Invalid image URL format", nameof(url));

        Id = id ?? throw new ArgumentNullException(nameof(id));
        Url = url;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
        AltText = altText;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Id;
    }
}