using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ExpressionBinaryAnd : ExpressionBinary
    {
        public override string Operator => "&&";
        public ExpressionBinaryAnd(BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public ExpressionBinaryAnd() : base() { }
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
            context.Errors.Add(new OperatorError(this, left?.GetType(), right?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new ExpressionBinaryAnd(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
