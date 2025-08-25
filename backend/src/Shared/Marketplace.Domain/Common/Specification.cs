using System.Linq.Expressions;

namespace Marketplace.Domain.Common;

/// <summary>
/// Base implementation of specification pattern
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
    
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
    
    // Specification composition methods
    public Specification<T> And(ISpecification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }
    
    public Specification<T> Or(ISpecification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }
    
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

// Composite specifications
internal class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        var left = Expression.Invoke(leftExpression, parameter);
        var right = Expression.Invoke(rightExpression, parameter);
        var and = Expression.AndAlso(left, right);
        
        return Expression.Lambda<Func<T, bool>>(and, parameter);
    }
}

internal class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();
        
        var parameter = Expression.Parameter(typeof(T));
        var left = Expression.Invoke(leftExpression, parameter);
        var right = Expression.Invoke(rightExpression, parameter);
        var or = Expression.OrElse(left, right);
        
        return Expression.Lambda<Func<T, bool>>(or, parameter);
    }
}

internal class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification;
    
    public NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }
    
    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var invoke = Expression.Invoke(expression, parameter);
        var not = Expression.Not(invoke);
        
        return Expression.Lambda<Func<T, bool>>(not, parameter);
    }
}