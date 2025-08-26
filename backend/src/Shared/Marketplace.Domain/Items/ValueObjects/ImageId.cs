using Marketplace.Domain.Common;

namespace Marketplace.Domain.Items.ValueObjects;

public class ImageId : ValueObject
{
    public Guid Value { get; }

    public ImageId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ImageId cannot be empty", nameof(value));
            
        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ImageId id) => id.Value;
}