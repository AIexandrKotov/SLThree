using Pegasus.Common;

namespace SLThree
{
    public class BoolLiteral : BoxLiteral<bool>
    {
        public BoolLiteral(bool value, Cursor cursor) : base(value, cursor) { }
        public BoolLiteral() : base() { }
        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetBool(Value);
            return ref reference;
        }
    }
}
