using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract class BaseStatement : ExecutionContext.IExecutable
    {
        public SourceContext SourceContext { get; set; }
        public BaseStatement() { }
        public BaseStatement(SourceContext context) => SourceContext = context;
        public abstract override string ToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
