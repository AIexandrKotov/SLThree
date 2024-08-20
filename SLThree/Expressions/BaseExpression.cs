using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract class BaseExpression : ExecutionContext.IExecutable
    {
        public bool PrioriryRaised { get; set; }
        public SourceContext SourceContext { get; set; }
        public BaseExpression() { }
        public BaseExpression(SourceContext context) => SourceContext = context;
        public BaseExpression(bool priority, SourceContext context) => (SourceContext, PrioriryRaised) = (context, priority);
        public override string ToString() => PrioriryRaised ? $"({ExpressionToString()})" : ExpressionToString();
        public abstract string ExpressionToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
