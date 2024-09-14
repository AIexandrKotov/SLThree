
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class LongLiteral : Literal<long>
    {
        public LongLiteral(long value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public LongLiteral(long value, ISourceContext cursor) : base(value, cursor) { }
        public LongLiteral() { }
        public override object Clone() => new LongLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation,
        };
    }
}
