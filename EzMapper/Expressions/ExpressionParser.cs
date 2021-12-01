using EzMapper.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Expressions
{
    internal static class ExpressionParser
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.binaryexpression?view=net-6.0

        private static readonly ExpressionVisitor _binaryVisitor = new RightHandValueResolver();

        public static string ParseExpression(Expression expression)
        {
            expression = _binaryVisitor.Visit(expression);
            return ParseExpressionIntern(expression);
        }

        private static string ParseExpressionIntern(Expression expression)
        {
            var sql = new StringBuilder();

            if (expression is BinaryExpression @binaryExpression)
            {
                var left = ParseExpressionIntern(binaryExpression.Left);
                var right = ParseExpressionIntern(binaryExpression.Right);
                var operand = binaryExpression.NodeType.ToSqlOperand();

                if (right == "NULL")
                {
                    // sqlite does not allow null checks with = and !=
                    if (operand == "=") operand = "IS ";
                    if (operand == "!=") operand = "IS NOT ";
                }

                sql.Append($"({left}) {operand} ({right})");
            }
            else if (expression is MemberExpression @member)
            {
                Type memberType = ((PropertyInfo)member.Member).PropertyType;

                if(Types.IsPrimitive(memberType))
                    sql.Append(member.Member.Name);
                else
                    sql.Append(member.Member.Name + "ID");

            }
            else if (expression is ConstantExpression @value)
            {
                if (value.Value is null)
                    sql.Append($"NULL");
                else
                    sql.Append(value.Value.GetType() == typeof(int) ? $" {value.Value} " : $" '{value.Value}' ");
            }
            else if (expression is UnaryExpression @unaryExpression)
            {
                sql.Append($" {unaryExpression.NodeType.ToSqlOperand()} ( {ParseExpressionIntern(unaryExpression.Operand)} ) ");
            }

            return sql.ToString();
        }
    }
}
