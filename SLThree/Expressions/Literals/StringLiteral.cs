using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Collections.Generic;

namespace SLThree
{
    public class StringLiteral : Literal<string>, IChooserExpression
    {
        public StringLiteral(string value, Cursor cursor) : base(value, $"\"{value}\"", cursor) { }
        public StringLiteral() : base() { }
        public override object Clone() => new StringLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast()
        };

        public object GetChooser(ExecutionContext context)
        {
            return new EqualchanceChooser<char>(RawRepresentation.ToCharArray());
        }
    }
}
