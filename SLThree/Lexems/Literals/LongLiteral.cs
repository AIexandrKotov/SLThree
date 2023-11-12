using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class LongLiteral : Literal<long>
    {
        public LongLiteral(long value, Cursor cursor) : base(value, cursor) { }
        public LongLiteral() { }
        public override object Clone() => new LongLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
