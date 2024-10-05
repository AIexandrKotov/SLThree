using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryRightShift : BinaryOperator
    {
        public override string Operator => ">>";
        public BinaryRightShift(BaseExpression left, BaseExpression right, ISourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public BinaryRightShift() : base() { }
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
                if (right is long i2) return i1 >> (int)i2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 >> (int)u2;
            }
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }

        public override object Clone()
        {
            return new BinaryRightShift(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
