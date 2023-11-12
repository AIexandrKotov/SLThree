using Pegasus.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract partial class BaseLexem : ExecutionContext.IExecutable, ICloneable
    {
        public SourceContext SourceContext { get; set; }
        public BaseLexem() { }
        public BaseLexem(SourceContext context) => SourceContext = context;
        public abstract override string ToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
