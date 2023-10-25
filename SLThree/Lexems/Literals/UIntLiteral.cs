using Pegasus.Common;

namespace SLThree
{
    public class UIntLiteral : Literal<uint>
    {
        public UIntLiteral(uint value, Cursor cursor) : base(value, cursor) { }
        public UIntLiteral() : base() { }
    }
}
