using System;

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
        public override string ExpressionToString()
        {
            var left = Left?.ToString() ?? "null";
            var right = Right?.ToString() ?? "null";
            if ((Left?.Priority ?? int.MinValue) > Priority)
                left = "(" + left + ")";
            if ((Right?.Priority ?? int.MinValue) > Priority)
                right = "(" + right + ")";
            return $"{left} {Operator} {right}";
        } 
        public override int Priority => Math.Max(Left.Priority, Right.Priority);
    }
}
