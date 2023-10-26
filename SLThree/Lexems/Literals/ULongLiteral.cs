using Pegasus.Common;

namespace SLThree
{
    public class ULongLiteral : BoxLiteral<ulong>
    {
        public ULongLiteral(ulong value, Cursor cursor) : base(value, cursor) { }
        public ULongLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetULong(Value);
            return ref reference;
        }
    }
}
