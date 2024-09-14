using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryAnd : BinaryOperator
    {
        public override string Operator => "&&";
        public BinaryAnd(BaseExpression left, BaseExpression right, ISourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public BinaryAnd() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left = Left.GetValue(context);
            object right = null;
            var right_counted = false;
            if (left is bool b1)
            {
                if (b1)
                {
                    right_counted = true;
                    if (Right.GetValue(context) is bool b2) return b2;
                }
                else return false;
            }
            right = right_counted ? right : Right.GetValue(context);
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }

        public override object Clone()
        {
            return new BinaryAnd(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
