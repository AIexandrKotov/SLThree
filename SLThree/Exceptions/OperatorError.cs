using SLThree.Extensions;
using System;

namespace SLThree
{
    public class OperatorError : RuntimeError
    {
        public OperatorError(UnaryOperator unary, Type left)
            : base(string.Format(Locale.Current["ERR_OPUnary"], unary.Operator, left?.GetTypeString() ?? "null"), unary.SourceContext) { }
        public OperatorError(BinaryOperator binary, Type left, Type right)
            : base(string.Format(Locale.Current["ERR_OPBinary"], binary.Operator, left?.GetTypeString() ?? "null", right?.GetTypeString() ?? "null"), binary.SourceContext) { }
        public OperatorError(string op, Type cond, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_OPTernary"], op, cond?.GetTypeString() ?? "null"), context) { }
        public OperatorError(string binary, Type left, Type right, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_OPBinary"], binary, left?.GetTypeString() ?? "null", right?.GetTypeString() ?? "null"), context) { }
    }
    public class DotnetSupportError : RuntimeError
    {
        public DotnetSupportError(string element, string start, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_DotnetSupport"], element, start), context) { }
    }
}
