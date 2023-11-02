using Pegasus.Common;

namespace SLThree
{
    public class ExpressionBinaryAnd : ExpressionBinary
    {
        public override string Operator => "&&";
        public ExpressionBinaryAnd(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryAnd() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left = Left.GetValue(context);
            object right = Right.GetValue(context);
            if (left is bool b1 && right is bool b2) return b1 && b2;
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }
    }
}
