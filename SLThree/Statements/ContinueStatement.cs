using Pegasus.Common;

namespace SLThree
{
    public class ContinueStatement : BaseStatement
    {
        public ContinueStatement(Cursor cursor) : base(cursor)
        {

        }

        public override string ToString() => $"continue";
        public override object GetValue(ExecutionContext context)
        {
            context.Continue();
            return null;
        }
    }
}
