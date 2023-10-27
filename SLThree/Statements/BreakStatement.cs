using Pegasus.Common;

namespace SLThree
{
    public class BreakStatement : BaseStatement
    {
        public BreakStatement(Cursor cursor) : base(cursor)
        {

        }

        public override string ToString() => $"break";
        public override object GetValue(ExecutionContext context)
        {
            context.Break();
            return null;
        }
    }
}
