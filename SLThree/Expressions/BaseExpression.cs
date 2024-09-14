using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {IISourceContext})")]
    public abstract class BaseExpression : ExecutionContext.IExecutable
    {
        public bool PrioriryRaised { get; set; }
        public ISourceContext SourceContext { get; set; }
        public BaseExpression() { }
        public BaseExpression(ISourceContext context) => SourceContext = context;
        public BaseExpression(bool priority, ISourceContext context) => (SourceContext, PrioriryRaised) = (context, priority);
        public override string ToString() => PrioriryRaised ? $"({ExpressionToString()})" : ExpressionToString();
        public abstract string ExpressionToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
