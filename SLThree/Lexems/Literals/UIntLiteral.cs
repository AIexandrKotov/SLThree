using Pegasus.Common;

namespace SLThree
{
    public class UIntLiteral : BoxLiteral<uint>
    {
        public UIntLiteral(uint value, Cursor cursor) : base(value, cursor) { }
        public UIntLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
