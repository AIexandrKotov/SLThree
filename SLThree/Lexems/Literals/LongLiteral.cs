using Pegasus.Common;

namespace SLThree
{
    public class LongLiteral : Literal<long>
    {
        public LongLiteral(long value, Cursor cursor) : base(value, cursor) { }
        public LongLiteral() { }
    }
}
