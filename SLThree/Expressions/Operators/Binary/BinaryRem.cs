using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryRem : BinaryOperator
    {
        public override string Operator => "-";
        public BinaryRem(BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public BinaryRem() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            object right;
            if (context.ForbidImplicit)
            {
                left = Left.GetValue(context);
                right = Right.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
                right = Right.GetValue(context).CastToMax();
            }
            if (left is long i1)
            {
                if (right is long i2) return i1 - i2;
                if (right is double d2) return i1 - d2;
            }
            else if (left is double d1)
            {
                if (right is double d2) return d1 - d2;
                if (right is long i2) return d1 - i2;
                if (right is ulong u2) return d1 - u2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 - u2;
                if (right is double d2) return u1 - d2;
            }
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }

        public override object Clone()
        {
            return new BinaryRem(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
