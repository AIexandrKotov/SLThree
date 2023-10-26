using Pegasus.Common;

namespace SLThree
{
    public class SByteLiteral : BoxLiteral<sbyte>
    {
        public SByteLiteral(sbyte value, Cursor cursor) : base(value, cursor) { }
        public SByteLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
