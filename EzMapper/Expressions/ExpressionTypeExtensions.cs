using System;
using System.Linq.Expressions;

namespace EzMapper.Expressions
{
    internal static class ExpressionTypeExtensions
    {
        public static string ToSqlOperand(this ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Equal: return "=";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                default: throw new InvalidOperationException("Operation not supported");

            }
        }

    }
}
