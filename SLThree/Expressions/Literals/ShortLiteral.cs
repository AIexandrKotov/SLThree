
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ShortLiteral : Literal<short>
    {
        public ShortLiteral(short value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public ShortLiteral(short value, ISourceContext cursor) : base(value, cursor) { }
        public ShortLiteral() : base() { }
        public override object Clone() => new ShortLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
