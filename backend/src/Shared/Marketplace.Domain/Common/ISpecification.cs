using System.Linq.Expressions;

namespace Marketplace.Domain.Common;

/// <summary>
/// Specification pattern interface for expressing business rules
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Returns the expression tree for the specification
    /// </summary>
    Expression<Func<T, bool>> ToExpression();
    
    /// <summary>
    /// Checks if the entity satisfies the specification
    /// </summary>
    bool IsSatisfiedBy(T entity);
}