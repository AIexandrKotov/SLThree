using System;

namespace SLThree
{
    public class UnsupportedTypesInUnaryExpression : RuntimeError
    {
        public UnsupportedTypesInUnaryExpression(ExpressionUnary unary, Type left)
            : base($"Unary operator {unary.Operator} not allow for {left?.Name ?? "null"}", unary.SourceContext) { }
    }
}
