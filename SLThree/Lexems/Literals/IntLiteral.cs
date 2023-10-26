using Pegasus.Common;

namespace SLThree
{
    public class IntLiteral : BoxLiteral<int>
    {
        public IntLiteral(int value, Cursor cursor) : base(value, cursor) { }
        public IntLiteral() : base() { }

        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
