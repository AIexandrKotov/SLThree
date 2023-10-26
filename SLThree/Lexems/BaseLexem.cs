using Pegasus.Common;
using System.Diagnostics;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract partial class BaseLexem
    {
        public SourceContext SourceContext { get; set; }
        public BaseLexem(Cursor cursor) => SourceContext = new SourceContext(cursor);
        public abstract override string ToString();
        public abstract SLTSpeedyObject GetValue(ExecutionContext context);
    }
}
