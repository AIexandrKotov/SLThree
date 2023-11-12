using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ULongLiteral : Literal<ulong>
    {
        public ULongLiteral(ulong value, Cursor cursor) : base(value, cursor) { }
        public ULongLiteral() : base() { }
        public override object Clone() => new ULongLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
