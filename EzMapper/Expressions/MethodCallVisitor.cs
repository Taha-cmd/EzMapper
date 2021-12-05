using System;
using System.Linq.Expressions;

namespace EzMapper.Expressions
{
    internal class MethodCallVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression call)
        {
            var owner = call.Object is null ? call.Arguments[0] : call.Object;
            var argument = call.Object is null ? call.Arguments[1] : call.Arguments[0];

            if (argument is MemberExpression @member)
            {
                var constant = Expression.Constant(Expression.Lambda(@member).Compile().DynamicInvoke());

                return Expression.Call(owner, call.Method, constant);
            }
            

            return base.VisitMethodCall(call);
        }
    }
}