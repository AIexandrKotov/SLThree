namespace SLThree
{
    public abstract class BinaryOperator : BaseExpression
    {
        public BaseExpression Left;
        public BaseExpression Right;

        public BinaryOperator(BaseExpression left, BaseExpression right, ISourceContext context) : base(context)
        {
            Left = left;
            Right = right;
        }
        public BinaryOperator() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString() => $"{Left?.ToString() ?? "null"} {Operator} {Right?.ToString() ?? "null"}";
    }
}
