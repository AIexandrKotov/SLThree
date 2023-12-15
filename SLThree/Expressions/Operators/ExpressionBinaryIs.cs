using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class ExpressionBinaryIs : ExpressionBinary
    {
        public ExpressionBinaryIs(BaseExpression left, BaseExpression right, SourceContext context) : base(left, right, context)
        {

        }

        public override string Operator => "is";

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context).GetType().IsType(Right.GetValue(context).Cast<Type>());
        }

        public override object Clone()
        {
            return new ExpressionBinaryIs(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
