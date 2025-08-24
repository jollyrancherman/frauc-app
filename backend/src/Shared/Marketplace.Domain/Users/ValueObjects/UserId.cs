using Marketplace.Domain.Common;

namespace Marketplace.Domain.Users.ValueObjects;

public sealed class UserId : ValueObject
{
    public Guid Value { get; private set; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static UserId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));

        return new UserId(value);
    }

    public static implicit operator Guid(UserId userId) => userId.Value;

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}