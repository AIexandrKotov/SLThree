using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BoolLiteral : Literal<bool>
    {
        public BoolLiteral(bool value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public BoolLiteral(bool value, ISourceContext cursor) : base(value, cursor) { }
        public BoolLiteral() : base() { }
        public override object Clone() => new BoolLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
