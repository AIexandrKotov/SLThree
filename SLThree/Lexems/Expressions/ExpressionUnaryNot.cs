using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryNot : ExpressionUnary
    {
        public override string Operator => "!";
        public ExpressionUnaryNot(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryNot() : base() { }
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
                case bool b: return !b;
            }
            throw new OperatorError(this, left?.GetType());
        }
    }
}
