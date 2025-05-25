using System.Linq.Expressions;

namespace Application.Common.Extensions
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Kết hợp hai biểu thức điều kiện (predicate) bằng toán tử AND.
        /// </summary>
        /// <typeparam name="T">Kiểu của đối tượng trong biểu thức.</typeparam>
        /// <param name="first">Biểu thức điều kiện thứ nhất.</param>
        /// <param name="second">Biểu thức điều kiện thứ hai.</param>
        /// <returns>Một biểu thức mới kết hợp cả hai điều kiện bằng AND.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var invokedExpr = Expression.Invoke(second, first.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(first.Body, invokedExpr), first.Parameters);
        }
    }
} 