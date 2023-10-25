using Pegasus.Common;

namespace SLThree
{
    public class SelfLiteral : BaseLexem
    {
        public SelfLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "self";

        public override object GetValue(ExecutionContext context) => context;
    }
}
