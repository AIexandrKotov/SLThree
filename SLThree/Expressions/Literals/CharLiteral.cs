
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class CharLiteral : Literal<char>
    {
        public CharLiteral(char value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public CharLiteral(char value, ISourceContext cursor) : base(value, $"'{value}'", cursor) { }
        public CharLiteral() : base() { }
        public override object Clone() => new CharLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
