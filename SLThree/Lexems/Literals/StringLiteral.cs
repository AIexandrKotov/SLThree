using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class StringLiteral : BoxLiteral<string>
    {
        public StringLiteral(string value, Cursor cursor) : base(value, cursor) { }
        public StringLiteral() : base() { }

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            return ref Value.ToSpeedy(ref reference);
        }
    }
}
