using Pegasus.Common;

namespace SLThree
{
    public class ByteLiteral : BoxLiteral<byte>
    {
        public ByteLiteral(byte value, Cursor cursor) : base(value, cursor) { }
        public ByteLiteral() : base() { }
        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
