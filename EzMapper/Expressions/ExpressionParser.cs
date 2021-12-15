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

        private static readonly ExpressionVisitor _valuesResolver1 = new BinaryVisitor();
        private static readonly ExpressionVisitor _valuesResolver2 = new MethodCallVisitor();

        public static string ParseExpression(Expression expression)
        {
            expression = _valuesResolver1.Visit(expression);
            expression = _valuesResolver2.Visit(expression);
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
                {
                    //check owner here for nested objects
                    sql.Append(member.Member.Name);  
                } 
                else
                    sql.Append(member.Type.Name + "ID");
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
            else if (expression is MethodCallExpression @call)
            {

                //calling contains on an array is an extensions method by linq
                //thus, arg 0 is the array iteself and arg 1 is the actual arugment
                //calling contains on a list is member method, thus, arg 0 is the argument

                var argument = call.Object is null ? call.Arguments[1] : call.Arguments[0];
                if (argument is not ConstantExpression) return sql.ToString();

                var argumentValue = ConstantExpressionValue(argument as ConstantExpression);

                if (!Types.IsPrimitive(argumentValue.GetType())) return sql.ToString();

                switch (call.Method.Name)
                {
                    case "Contains":
                        var memeberExpression = (call.Object is null ? call.Arguments[0] : call.Object) as MemberExpression;

                        string propOwner = memeberExpression.Member.DeclaringType.Name;
                        Type propHolder = memeberExpression.Expression.Type;
                        string holderPk = ModelParser.GetPkFieldName(propHolder);
                        string propName = memeberExpression.Member.Name;
                        string collectionTableName = propOwner + propName;

                        sql.Append($" ( \"{argumentValue}\" IN (SELECT {collectionTableName}.{propName} FROM {collectionTableName} WHERE {collectionTableName}.{propOwner}ID = {propHolder.Name}_{holderPk}) ) ");

                        //WHERE  "Swimming" IN(SELECT PersonHobbies.Hobbies FROM PersonHobbies WHERE PersonHobbies.PersonID = Teacher_PersonID)


                    break;
                }
            }

            return sql.ToString();
        }

        private static object ConstantExpressionValue(ConstantExpression expression)
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
    }
}
