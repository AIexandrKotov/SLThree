using Pegasus.Common;

namespace SLThree
{
    public class CharLiteral : Literal<char>
    {
        public CharLiteral(char value, Cursor cursor) : base(value, cursor) { }
        public CharLiteral() : base() { }
    }
}
