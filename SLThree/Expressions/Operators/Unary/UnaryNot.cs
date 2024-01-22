using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UnaryNot : UnaryOperator
    {
        public override string Operator => "!";
        public UnaryNot(BaseExpression left, SourceContext context, bool priority = false) : base(left, context, priority) { }
        public UnaryNot() : base() { }
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
            return new UnaryNot(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
