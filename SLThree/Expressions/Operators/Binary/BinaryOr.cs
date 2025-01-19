using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryOr : BinaryOperator
    {
        public override int Priority => 7;
        public override string Operator => "||";
        public BinaryOr(BaseExpression left, BaseExpression right, ISourceContext context) : base(left, right, context) { }
        public BinaryOr() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left = Left.GetValue(context);
            object right = null;
            var right_counted = false;
            if (left is bool b1)
            {
                if (b1) return true;
                else
                {
                    right_counted = true;
                    if (Right.GetValue(context) is bool b2) return b2;
                }
            }
            right = right_counted ? right : Right.GetValue(context);
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }

        public override object Clone()
        {
            return new BinaryOr(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
