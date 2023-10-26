using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionBinaryEquals : ExpressionBinary
    {
        public override string Operator => "==";
        public ExpressionBinaryEquals(BoxSupportedLexem left, BoxSupportedLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryEquals() : base() { }
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetBoxValue(context);
            var right = Right.GetBoxValue(context);
            if (left.Type == SLTSpeedyObject.DoubleType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsDouble == right.AsDouble);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.ULongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsDouble == right.AsULong);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.LongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsDouble == right.AsLong);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.BoolType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsDouble != 0 == right.AsBool);
                    return ref reference;
                }
            }
            if (left.Type == SLTSpeedyObject.ULongType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsULong == right.AsDouble);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.ULongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsULong == right.AsULong);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.BoolType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsULong != 0 == right.AsBool);
                    return ref reference;
                }
            }
            if (left.Type == SLTSpeedyObject.LongType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsLong == right.AsDouble);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.LongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsLong == right.AsLong);
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.BoolType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsLong != 0 == right.AsBool);
                    return ref reference;
                }
            }
            if (left.Type == SLTSpeedyObject.BoolType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsBool == (right.AsDouble != 0));
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.ULongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsBool == (right.AsULong != 0));
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.LongType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsBool == (right.AsLong != 0));
                    return ref reference;
                }
                if (right.Type == SLTSpeedyObject.BoolType)
                {
                    reference = SLTSpeedyObject.GetBool(left.AsBool == right.AsBool);
                    return ref reference;
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left.Boxed()?.GetType(), right.Boxed()?.GetType());
        }
    }
}
