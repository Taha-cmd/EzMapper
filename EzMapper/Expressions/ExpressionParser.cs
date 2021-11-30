using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Expressions
{
    internal static class ExpressionParser
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.binaryexpression?view=net-6.0
        public static string ParseExpression(Expression expression)
        {

            //TODO: parse unary operations, parse non primitves
            var sql = new StringBuilder();

            if (expression is BinaryExpression @binaryExpression)
            {
                if (binaryExpression.Left is BinaryExpression)
                {
                    sql.Append(ParseExpression(binaryExpression.Left));
                    sql.Append($" {binaryExpression.NodeType.ToSqlOperand()} ");
                }

                if (binaryExpression.Right is BinaryExpression)
                {
                    sql.Append(ParseExpression(binaryExpression.Right));
                }

                if (binaryExpression.Left is MemberExpression @member)
                {
                    sql.Append($"{member.Member.Name} {binaryExpression.NodeType.ToSqlOperand()}  ");

                    if (binaryExpression.Right is ConstantExpression @value)
                        sql.Append(value.Value.GetType() == typeof(int) ? $" {value.Value} " : $" '{value.Value}' ");
                    else if (binaryExpression.Right is MemberExpression @variable)
                        sql.Append(GetMemberValue(variable).GetType() == typeof(int) ? $" {GetMemberValue(variable)} " : $" '{GetMemberValue(variable)}' ");
                }
            }

            return sql.ToString();
        }

        private static object GetMemberValue(MemberExpression memberExpression)
        {
            return Expression.Lambda(memberExpression).Compile().DynamicInvoke();
        }
    }
}
