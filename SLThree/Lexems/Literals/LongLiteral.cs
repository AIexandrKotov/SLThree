using Pegasus.Common;

namespace SLThree
{
    public class LongLiteral : BoxLiteral<long>
    {
        public LongLiteral(long value, Cursor cursor) : base(value, cursor) { }
        public LongLiteral() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetLong(Value);
            return ref reference;
        }
    }
}
