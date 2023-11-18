using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UIntLiteral : Literal<uint>
    {
        public UIntLiteral(uint value, string raw, Cursor cursor) : base(value, raw, cursor) { }
        public UIntLiteral(uint value, Cursor cursor) : base(value, cursor) { }
        public UIntLiteral() : base() { }
        public override object Clone() => new UIntLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
