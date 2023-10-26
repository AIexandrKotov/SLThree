using Pegasus.Common;

namespace SLThree
{
    public class GlobalLiteral : BoxSupportedLexem
    {
        public GlobalLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "global";

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(ExecutionContext.global);
            return ref reference;
        }
    }
}
