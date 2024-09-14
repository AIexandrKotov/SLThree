
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class SByteLiteral : Literal<sbyte>
    {
        public SByteLiteral(sbyte value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public SByteLiteral(sbyte value, ISourceContext cursor) : base(value, cursor) { }
        public SByteLiteral() : base() { }
        public override object Clone() => new SByteLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
