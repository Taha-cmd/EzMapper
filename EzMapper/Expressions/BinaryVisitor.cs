using System.Linq.Expressions;

namespace EzMapper.Expressions
{
    internal class BinaryVisitor : ExpressionVisitor
    {
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
