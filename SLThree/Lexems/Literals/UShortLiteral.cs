using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UShortLiteral : Literal<ushort>
    {
        public UShortLiteral(ushort value, Cursor cursor) : base(value, cursor) { }
        public UShortLiteral() : base() { }
        public override object Clone() => new UShortLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
