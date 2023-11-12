using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class CharLiteral : Literal<char>
    {
        public CharLiteral(char value, Cursor cursor) : base(value, cursor) { }
        public CharLiteral() : base() { }
        public override object Clone() => new CharLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
