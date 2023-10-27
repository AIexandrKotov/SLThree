using Pegasus.Common;

namespace SLThree
{
    public class ByteLiteral : Literal<byte>
    {
        public ByteLiteral(byte value, Cursor cursor) : base(value, cursor) { }
        public ByteLiteral() : base() { }
    }
}
