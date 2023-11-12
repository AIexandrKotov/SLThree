using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BreakStatement : BaseStatement
    {
        public BreakStatement(SourceContext context) : base(context) { }
        public BreakStatement(Cursor cursor) : base(cursor) { }

        public override string ToString() => $"break";
        public override object GetValue(ExecutionContext context)
        {
            context.Break();
            return null;
        }
        public override object Clone() => new BreakStatement(SourceContext.CloneCast());
    }
}
