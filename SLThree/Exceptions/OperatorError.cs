using SLThree.Extensions;
using System;

namespace SLThree
{
    public class OperatorError : RuntimeError
    {
        public OperatorError(UnaryOperator unary, Type left)
            : base($"Operator {unary.Operator} not allow for {left?.GetTypeString() ?? "null"}", unary.SourceContext) { }
        public OperatorError(BinaryOperator binary, Type left, Type right)
            : base($"Operator {binary.Operator} not allow for {left?.GetTypeString() ?? "null"} and {right?.GetTypeString() ?? "null"}", binary.SourceContext) { }
        public OperatorError(string op, Type cond, SourceContext context)
            : base($"Operator {op} not allow for {cond?.GetTypeString() ?? "null"}", context) { }
        public OperatorError(string binary, Type left, Type right, SourceContext context)
            : base($"Operator {binary} not allow for {left?.GetTypeString() ?? "null"} and {right?.GetTypeString() ?? "null"}", context) { }
    }
}
