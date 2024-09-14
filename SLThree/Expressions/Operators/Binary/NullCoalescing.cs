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
            var left = Left.GetValue(context);
            if (left == null) return Right.GetValue(context);
            return left;
        }

        public override object Clone()
        {
            return new NullCoalescing(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
