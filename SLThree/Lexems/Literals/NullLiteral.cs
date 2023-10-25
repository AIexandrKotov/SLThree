using Pegasus.Common;

namespace SLThree
{
    public class NullLiteral : BaseLexem
    {
        public NullLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "null";

        public override object GetValue(ExecutionContext context) => null;
    }
}
