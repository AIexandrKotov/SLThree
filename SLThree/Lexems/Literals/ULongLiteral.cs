using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ULongLiteral : Literal<ulong>
    {
        public ULongLiteral(ulong value, string raw, Cursor cursor) : base(value, raw, cursor) { }
        public ULongLiteral(ulong value, Cursor cursor) : base(value, cursor) { }
        public ULongLiteral() : base() { }
        public override object Clone() => new ULongLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
