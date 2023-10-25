using Pegasus.Common;

namespace SLThree
{
    public class ShortLiteral : Literal<short>
    {
        public ShortLiteral(short value, Cursor cursor) : base(value, cursor) { }
        public ShortLiteral() : base() { }
    }
}
