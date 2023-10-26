using Pegasus.Common;

namespace SLThree
{
    public class NullLiteral : BoxSupportedLexem
    {
        public NullLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "null";

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(null);
            return ref reference;
        }
    }
}
