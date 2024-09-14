
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UIntLiteral : Literal<uint>
    {
        public UIntLiteral(uint value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public UIntLiteral(uint value, ISourceContext cursor) : base(value, cursor) { }
        public UIntLiteral() : base() { }
        public override object Clone() => new UIntLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
