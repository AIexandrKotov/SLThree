using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryRem : ExpressionUnary
    {
        public override string Operator => "-";
        public ExpressionUnaryRem(BoxSupportedLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryRem() : base() { }
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetBoxValue(context);

            if (left.Type == SLTSpeedyObject.DoubleType)
            {
                reference.AsDouble = -left.AsDouble;
                return ref reference;
            }

            if (left.Type == SLTSpeedyObject.LongType)
            {
                reference.AsLong = -left.AsLong;
                return ref reference;
            }

            throw new UnsupportedTypesInUnaryExpression(this, left.Boxed()?.GetType());
        }
    }
}
