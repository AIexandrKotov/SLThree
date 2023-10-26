using Pegasus.Common;

namespace SLThree
{
    public class ShortLiteral : BoxLiteral<short>
    {
        public ShortLiteral(short value, Cursor cursor) : base(value, cursor) { }
        public ShortLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
