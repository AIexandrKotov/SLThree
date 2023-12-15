using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ExpressionUnaryNot : ExpressionUnary
    {
        public override string Operator => "!";
        public ExpressionUnaryNot(BaseExpression left, SourceContext context, bool priority = false) : base(left, context, priority) { }
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
            context.Errors.Add(new OperatorError(this, left?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new ExpressionUnaryNot(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
