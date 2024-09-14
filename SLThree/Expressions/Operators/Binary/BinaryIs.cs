using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class BinaryIs : BinaryOperator
    {
        public BinaryIs(BaseExpression left, BaseExpression right, ISourceContext context) : base(left, right, context)
        {

        }

        public override string Operator => "is";

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context)?.GetType().IsType(Right.GetValue(context).Cast<Type>()) ?? false;
        }

        public override object Clone()
        {
            return new BinaryIs(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
