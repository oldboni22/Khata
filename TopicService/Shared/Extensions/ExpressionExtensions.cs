using System.Linq.Expressions;

namespace Shared.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> source, Expression<Func<T, bool>> addition)
    {
        var parameter = source.Parameters[0];
        var invokedAddition = Expression.Invoke(addition, parameter);
        
        var newBody = Expression.AndAlso(source.Body, invokedAddition);
        return Expression.Lambda<Func<T, bool>>(newBody, parameter);
    }
}
