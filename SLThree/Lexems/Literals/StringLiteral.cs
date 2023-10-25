using Pegasus.Common;

namespace SLThree
{
    public class StringLiteral : Literal<string>
    {
        public StringLiteral(string value, Cursor cursor) : base(value, cursor) { }
        public StringLiteral() : base() { }
    }
}
