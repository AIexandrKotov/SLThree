using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryAdd : ExpressionUnary
    {
        public override string Operator => "+";
        public ExpressionUnaryAdd(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryAdd() : base() { }
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
                case long v: return +v;
                case ulong v: return +v;
                case double v: return +v;
            }
            throw new OperatorError(this, left?.GetType());
        }
    }
}
