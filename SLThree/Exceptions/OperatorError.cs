using System;

namespace SLThree
{
    public class OperatorError : RuntimeError
    {
        public OperatorError(ExpressionUnary unary, Type left)
            : base($"Operator {unary.Operator} not allow for {left?.Name ?? "null"}", unary.SourceContext) { }
        public OperatorError(ExpressionBinary binary, Type left, Type right)
            : base($"Operator {binary.Operator} not allow for {left?.Name ?? "null"} and {right?.Name ?? "null"}", binary.SourceContext) { }
    }
}
