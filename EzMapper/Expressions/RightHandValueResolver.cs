using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EzMapper.Expressions
{
    internal class RightHandValueResolver : ExpressionVisitor
    {
        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Right is MemberExpression @member)
                return Expression.MakeBinary(binaryExpression.NodeType, binaryExpression.Left, MemberToConstantExpression(member));

            return base.VisitBinary(binaryExpression);
        }

        private ConstantExpression MemberToConstantExpression(MemberExpression memberExpression)
        {
            return Expression.Constant(Expression.Lambda(memberExpression).Compile().DynamicInvoke());
        }
    }
}
