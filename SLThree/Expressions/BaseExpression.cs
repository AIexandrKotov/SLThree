using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract class BaseExpression : ExecutionContext.IExecutable
    {
        public ISourceContext SourceContext { get; set; }
        public BaseExpression() { }
        public BaseExpression(ISourceContext context) => SourceContext = context;
        public override string ToString() => ExpressionToString();
        public abstract string ExpressionToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
