using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UnaryRem : UnaryOperator
    {
        public override string Operator => "-";
        public UnaryRem(BaseExpression left, SourceContext context, bool priority = false) : base(left, context, priority) { }
        public UnaryRem() : base() { }
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
            context.Errors.Add(new OperatorError(this, left?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new UnaryRem(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
