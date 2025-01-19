using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UnaryRem : UnaryOperator
    {
        public override string Operator => "-";
        public UnaryRem(BaseExpression left, ISourceContext context) : base(left, context) { }
        public UnaryRem() : base() { }
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
                case long v: return -v;
                case double v: return -v;
            }
            throw new OperatorError(this, left?.GetType());
        }

        public override object Clone()
        {
            return new UnaryRem(Left.CloneCast(), SourceContext.CloneCast());
        }
    }
}
