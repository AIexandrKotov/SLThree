using Pegasus.Common;

namespace SLThree
{
    public class BoolLiteral : Literal<bool>
    {
        public BoolLiteral(bool value, Cursor cursor) : base(value, cursor) { }
        public BoolLiteral() : base() { }
    }
}
