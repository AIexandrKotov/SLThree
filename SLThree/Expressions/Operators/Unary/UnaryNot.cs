using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UnaryNot : UnaryOperator
    {
        public override string Operator => "!";
        public UnaryNot(BaseExpression left, ISourceContext context) : base(left, context) { }
        public UnaryNot() : base() { }
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
                case bool b: return !b;
            }
            throw new OperatorError(this, left?.GetType());
        }

        public override object Clone()
        {
            return new UnaryNot(Left.CloneCast(), SourceContext.CloneCast());
        }
    }
}
