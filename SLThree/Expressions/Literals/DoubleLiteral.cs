
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class DoubleLiteral : Literal<double>
    {
        public DoubleLiteral(double value, string raw, ISourceContext cursor) : base(value, raw, cursor) { }
        public DoubleLiteral(double value, ISourceContext cursor) : base(value, cursor) { }
        public DoubleLiteral() : base() { }
        public override object Clone() => new DoubleLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
