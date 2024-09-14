
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ByteLiteral : Literal<byte>
    {
        public ByteLiteral(byte value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public ByteLiteral(byte value, ISourceContext cursor) : base(value, cursor) { }
        public ByteLiteral() : base() { }
        public override object Clone() => new ByteLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
