namespace SLThree
{
    public abstract class UnaryOperator : BaseExpression
    {
        public BaseExpression Left;
        public UnaryOperator(BaseExpression left, SourceContext context) : base(context)
        {
            Left = left;
        }
        public UnaryOperator(BaseExpression left, SourceContext context, bool priority) : base(priority, context)
        {
            Left = left;
        }
        public UnaryOperator() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString() => $"{Operator}{Left?.ToString() ?? "null"}";
    }
}
