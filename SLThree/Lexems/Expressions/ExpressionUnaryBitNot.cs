using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryBitNot : ExpressionUnary
    {
        public override string Operator => "~";
        public ExpressionUnaryBitNot(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryBitNot() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            if (context.ForbidImplicit)
            {
                left = Left.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
            }
            switch (left)
            {
                case long v: return ~v;
                case ulong v: return ~v;
            }
            throw new OperatorError(this, left?.GetType());
        }
    }
}
