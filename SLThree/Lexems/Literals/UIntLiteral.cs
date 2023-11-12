using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class UIntLiteral : Literal<uint>
    {
        public UIntLiteral(uint value, Cursor cursor) : base(value, cursor) { }
        public UIntLiteral() : base() { }
        public override object Clone() => new UIntLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
