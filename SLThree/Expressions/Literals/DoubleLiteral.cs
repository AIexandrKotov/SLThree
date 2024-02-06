using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class DoubleLiteral : Literal<double>
    {
        public DoubleLiteral(double value, string raw, Cursor cursor) : base(value, raw, cursor) { }
        public DoubleLiteral(double value, Cursor cursor) : base(value, cursor) { }
        public DoubleLiteral() : base() { }
        public override object Clone() => new DoubleLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
