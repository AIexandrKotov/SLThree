using Pegasus.Common;

namespace SLThree
{
    public class UShortLiteral : BoxLiteral<ushort>
    {
        public UShortLiteral(ushort value, Cursor cursor) : base(value, cursor) { }
        public UShortLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
