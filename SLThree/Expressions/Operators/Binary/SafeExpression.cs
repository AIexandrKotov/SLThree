using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class SafeExpression : BinaryOperator
    {
        public override int Priority => 8;
        public override string Operator => "-?";
        public SafeExpression(BaseExpression left, BaseExpression right, ISourceContext context) : base(left, right, context) { }
        public SafeExpression() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            try
            {
                return Left.GetValue(context);
            }
            catch
            {
                return Right.GetValue(context);
            }
        }

        public override object Clone()
        {
            return new SafeExpression(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
