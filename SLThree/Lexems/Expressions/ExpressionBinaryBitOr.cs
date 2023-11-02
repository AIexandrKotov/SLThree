using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionBinaryBitOr : ExpressionBinary
    {
        public override string Operator => "|";
        public ExpressionBinaryBitOr(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryBitOr() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            object right;
            if (context.fimp)
            {
                left = Left.GetValue(context);
                right = Right.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
                right = Right.GetValue(context).CastToMax();
            }
            if (left is bool b1 && right is bool b2) return b1 | b2;
            else if (left is long i1)
            {
                if (right is long i2) return i1 | i2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 | u2;
            }
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }
    }
}
