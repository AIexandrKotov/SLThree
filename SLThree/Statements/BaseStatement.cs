using Pegasus.Common;
using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract class BaseStatement : ExecutionContext.IExecutable
    {
        public SourceContext SourceContext { get; set; }
        public BaseStatement(Cursor cursor) => SourceContext = new SourceContext(cursor);
        public abstract override string ToString();
        public abstract object GetValue(ExecutionContext context);
    }
}
