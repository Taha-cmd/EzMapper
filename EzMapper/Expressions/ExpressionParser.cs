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
            

            var sql = new StringBuilder();

            if (expression is BinaryExpression @binaryExpression)
            {
                sql.Append($"({ParseExpression(binaryExpression.Left)}) {binaryExpression.NodeType.ToSqlOperand()} ({ParseExpression(binaryExpression.Right)})");
            }
            else if(expression is MemberExpression @member)
            {
                sql.Append(member.Member.Name);    //problem: differentitate between actual member and a variable
            }
            else if (expression is ConstantExpression @value)
            {
                sql.Append(value.Value.GetType() == typeof(int) ? $" {value.Value} " : $" '{value.Value}' ");
            }
            else if(expression is UnaryExpression @unaryExpression)
            {
                sql.Append($" {unaryExpression.NodeType.ToSqlOperand()} ( {ParseExpression(unaryExpression.Operand)} ) ");
            }

            return sql.ToString();
        }


        private static object GetMemberValue(MemberExpression memberExpression)
        {
            return Expression.Lambda(memberExpression).Compile().DynamicInvoke();
        }
    }
}
