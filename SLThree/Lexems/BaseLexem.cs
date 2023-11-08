using Pegasus.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract partial class BaseLexem : ExecutionContext.IExecutable
    {
        public SourceContext SourceContext { get; set; }
        public BaseLexem(Cursor cursor) => SourceContext = new SourceContext(cursor);
        public abstract override string ToString();
        public abstract object GetValue(ExecutionContext context);
    }
}
