using Marketplace.Domain.Common;

namespace Marketplace.Domain.Items.ValueObjects;

public class ItemId : ValueObject
{
    public Guid Value { get; }

    public ItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ItemId cannot be empty", nameof(value));
            
        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ItemId id) => id.Value;
}