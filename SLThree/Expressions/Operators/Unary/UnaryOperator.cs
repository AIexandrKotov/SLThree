namespace SLThree
{
    public abstract class UnaryOperator : BaseExpression
    {
        public BaseExpression Left;
        public UnaryOperator(BaseExpression left, ISourceContext context) : base(context)
        {
            Left = left;
        }
        public UnaryOperator() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString() => $"{Operator}{Left?.ToString() ?? "null"}";
    }
}
