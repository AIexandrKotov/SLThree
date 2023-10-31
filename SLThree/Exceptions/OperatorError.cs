using SLThree.Extensions;
using System;

namespace SLThree
{
    public class OperatorError : RuntimeError
    {
        public OperatorError(ExpressionUnary unary, Type left)
            : base($"Operator {unary.Operator} not allow for {left?.GetTypeString() ?? "null"}", unary.SourceContext) { }
        public OperatorError(ExpressionBinary binary, Type left, Type right)
            : base($"Operator {binary.Operator} not allow for {left?.GetTypeString() ?? "null"} and {right?.GetTypeString() ?? "null"}", binary.SourceContext) { }
    }
}
