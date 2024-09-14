
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class FloatLiteral : Literal<float>
    {
        public FloatLiteral(float value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public FloatLiteral(float value, ISourceContext cursor) : base(value, cursor) { }
        public FloatLiteral() : base() { }
        public override object Clone() => new FloatLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
