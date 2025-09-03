using System.Linq.Expressions;

namespace Shared.Extensions;

public static class ExpressionExtensions
{
    // Честно, эта часть это уже полностью из ии
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        // Получаем параметр из первого выражения (например, "t =>")
        var parameter = left.Parameters[0];

        // Создаем визитор, который заменит параметр во втором выражении
        // на параметр из первого
        var visitor = new ReplaceParameterVisitor(right.Parameters[0], parameter);
        var newRightBody = visitor.Visit(right.Body);

        // Объединяем тела выражений через логическое "И"
        var newBody = Expression.AndAlso(left.Body, newRightBody);

        // Создаем новую лямбду с объединенным телом и оригинальным параметром
        return Expression.Lambda<Func<T, bool>>(newBody, parameter);
    }

    private class ReplaceParameterVisitor(
        ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            // Если узел - это тот параметр, который мы хотим заменить, заменяем его
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }
}
