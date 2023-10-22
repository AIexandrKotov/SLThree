using System;

namespace SLThree
{
    public class UnsupportedTypesInBinaryExpression : RuntimeError
    {
        public UnsupportedTypesInBinaryExpression(ExpressionBinary binary, Type left, Type right)
            : base($"Operator {binary.Operator} not allow for {left?.Name ?? "null"} and {right?.Name ?? "null"}", binary.SourceContext) { }
    }
}
