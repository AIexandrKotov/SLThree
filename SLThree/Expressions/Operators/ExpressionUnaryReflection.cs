namespace SLThree
{
    public class ExpressionUnaryReflection : ExpressionUnary
    {
        public ExpressionUnaryReflection(BaseExpression left, SourceContext context) : base(left, context)
        {
        }

        public override string Operator => "@";

        public override object GetValue(ExecutionContext context)
        {
            return Left.GetValue(context);
        }

        public override object Clone()
        {
            return new ExpressionUnaryReflection(Left, SourceContext);
        }
    }
}