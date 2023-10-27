using Pegasus.Common;

namespace SLThree
{
    public class SByteLiteral : Literal<sbyte>
    {
        public SByteLiteral(sbyte value, Cursor cursor) : base(value, cursor) { }
        public SByteLiteral() : base() { }
    }
}
