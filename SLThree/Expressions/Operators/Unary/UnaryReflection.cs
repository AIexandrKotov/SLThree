namespace SLThree
{
    public class UnaryReflection : UnaryOperator
    {
        public UnaryReflection(BaseExpression left, ISourceContext context) : base(left, context)
        {
        }

        public override string Operator => "@";

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context);
        }

        public override object Clone()
        {
            return new UnaryReflection(Left, SourceContext);
        }
    }
}