
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ULongLiteral : Literal<ulong>
    {
        public ULongLiteral(ulong value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public ULongLiteral(ulong value, ISourceContext cursor) : base(value, cursor) { }
        public ULongLiteral() : base() { }
        public override object Clone() => new ULongLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
