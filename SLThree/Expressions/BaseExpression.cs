using Pegasus.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract partial class BaseExpression : ExecutionContext.IExecutable, ICloneable
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
