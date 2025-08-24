using Marketplace.Domain.Common;

namespace Marketplace.Domain.Items.ValueObjects;

public class UserId : ValueObject
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));
            
        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
}