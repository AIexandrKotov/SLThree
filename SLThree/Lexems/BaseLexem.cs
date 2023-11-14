using Pegasus.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SLThree
{
    [DebuggerDisplay("{ToString()}        ([{GetType().Name}] at {SourceContext})")]
    public abstract partial class BaseLexem : ExecutionContext.IExecutable, ICloneable
    {
        public bool PrioriryRaised { get; set; }
        public SourceContext SourceContext { get; set; }
        public BaseLexem() { }
        public BaseLexem(SourceContext context) => SourceContext = context;
        public BaseLexem(bool priority, SourceContext context) => (SourceContext, PrioriryRaised) = (context, priority);
        public override string ToString() => PrioriryRaised ? $"({LexemToString()})" : LexemToString();
        public abstract string LexemToString();
        public abstract object GetValue(ExecutionContext context);
        public abstract object Clone();
    }
}
