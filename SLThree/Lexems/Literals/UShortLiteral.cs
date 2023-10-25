using Pegasus.Common;

namespace SLThree
{
    public class UShortLiteral : Literal<ushort>
    {
        public UShortLiteral(ushort value, Cursor cursor) : base(value, cursor) { }
        public UShortLiteral() : base() { }
    }
}
