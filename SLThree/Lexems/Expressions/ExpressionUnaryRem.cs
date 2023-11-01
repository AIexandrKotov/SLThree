using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryRem : ExpressionUnary
    {
        public override string Operator => "-";
        public ExpressionUnaryRem(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryRem() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            if (context.fimp)
            {
                left = Left.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
            }
            switch (left)
            {
                case long v: return -v;
                case double v: return -v;
            }
            throw new OperatorError(this, left?.GetType());
        }
    }
}
