using Marketplace.Domain.Common;

namespace Marketplace.Domain.Listings.ValueObjects;

public sealed class ListingId : ValueObject
{
    public Guid Value { get; }

    public ListingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Listing ID cannot be empty", nameof(value));

        Value = value;
    }

    public static ListingId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ListingId listingId) => listingId.Value;
    public static explicit operator ListingId(Guid value) => new(value);
}