using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class SelfLiteral : BoxSupportedLexem
    {
        public SelfLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "self";

        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            return ref context.ToSpeedy(ref reference);
        }
    }
}
