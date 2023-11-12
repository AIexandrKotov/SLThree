using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class SByteLiteral : Literal<sbyte>
    {
        public SByteLiteral(sbyte value, Cursor cursor) : base(value, cursor) { }
        public SByteLiteral() : base() { }
        public override object Clone() => new SByteLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
