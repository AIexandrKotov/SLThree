using System;

namespace SLThree
{
    public abstract class UnaryOperator : BaseExpression
    {
        public override int Priority => -1;
        public BaseExpression Left;
        public UnaryOperator(BaseExpression left, ISourceContext context) : base(context)
        {
            Left = left;
        }
        public UnaryOperator() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString()
        {
            var left = Left?.ToString() ?? "null";
            if ((Left?.Priority ?? int.MinValue) > Priority)
                left = "(" + left + ")";
            return $"{Operator}{left}";
        }
    }
}
