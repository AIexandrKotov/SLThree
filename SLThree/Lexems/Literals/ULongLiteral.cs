using Pegasus.Common;

namespace SLThree
{
    public class ULongLiteral : Literal<ulong>
    {
        public ULongLiteral(ulong value, Cursor cursor) : base(value, cursor) { }
        public ULongLiteral() : base() { }
    }
}
