using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryBitNot : ExpressionUnary
    {
        public override string Operator => "~";
        public ExpressionUnaryBitNot(BoxSupportedLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryBitNot() : base() { }
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetBoxValue(context);

            if (left.Type == SLTSpeedyObject.LongType)
            {
                reference.AsLong = ~left.AsLong;
                return ref reference;
            }
            if (left.Type == SLTSpeedyObject.ULongType)
            {
                reference.AsULong = ~left.AsULong;
                return ref reference;
            }

            throw new UnsupportedTypesInUnaryExpression(this, left.Boxed()?.GetType());
        }
    }
}
