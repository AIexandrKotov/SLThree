using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class NullCoalescing : BinaryOperator
    {
        public override string Operator => "??";
        public NullCoalescing(BaseExpression left, BaseExpression right, ISourceContext context) : base(left, right, context) { }
        public NullCoalescing() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context) ?? Right.GetValue(context);
        }

        public override object Clone()
        {
            return new NullCoalescing(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
