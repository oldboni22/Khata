using System.Linq.Expressions;

namespace Shared.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters[0];
        
        var visitor = new ReplaceParameterVisitor(right.Parameters[0], parameter);
        var newRightBody = visitor.Visit(right.Body);
        
        var newBody = Expression.AndAlso(left.Body, newRightBody);
        
        return Expression.Lambda<Func<T, bool>>(newBody, parameter);
    }

    private class ReplaceParameterVisitor(
        ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
}
