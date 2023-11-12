using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class IntLiteral : Literal<int>
    {
        public IntLiteral(int value, Cursor cursor) : base(value, cursor) { }
        public IntLiteral() : base() { }
        public override object Clone() => new IntLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
