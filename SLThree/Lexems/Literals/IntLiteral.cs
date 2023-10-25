using Pegasus.Common;

namespace SLThree
{
    public class IntLiteral : Literal<int>
    {
        public IntLiteral(int value, Cursor cursor) : base(value, cursor) { }
        public IntLiteral() : base() { }
    }
}
