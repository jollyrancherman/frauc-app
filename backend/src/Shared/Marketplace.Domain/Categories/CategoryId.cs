using Marketplace.Domain.Common;

namespace Marketplace.Domain.Categories;

public class CategoryId : ValueObject
{
    public Guid Value { get; }

    public CategoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CategoryId cannot be empty", nameof(value));
            
        Value = value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CategoryId id) => id.Value;
}