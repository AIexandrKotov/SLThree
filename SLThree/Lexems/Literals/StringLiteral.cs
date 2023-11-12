using Pegasus.Common;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class StringLiteral : Literal<string>
    {
        public StringLiteral(string value, Cursor cursor) : base(value, cursor) { }
        public StringLiteral() : base() { }
        public override object Clone() => new StringLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
