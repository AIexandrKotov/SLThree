using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class NullCoalescing : BinaryOperator
    {
        public override string Operator => "??";
        public NullCoalescing(BaseExpression left, BaseExpression right, ISourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public NullCoalescing() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context) ?? Right.GetValue(context);
        }

        public override object Clone()
        {
            return new NullCoalescing(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
