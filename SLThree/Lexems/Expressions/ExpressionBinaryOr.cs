using Pegasus.Common;

namespace SLThree
{
    public class ExpressionBinaryOr: ExpressionBinary
    {
        public override string Operator => "||";
        public ExpressionBinaryOr(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryOr() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left = Left.GetValue(context);
            object right = Right.GetValue(context);
            if (left is bool b1 && right is bool b2) return b1 || b2;
            context.Errors.Add(new OperatorError(this, left?.GetType(), right?.GetType()));
            return null;
        }
    }
}
