using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SLThree
{
    public class ExpressionBinaryRem : ExpressionBinary
    {
        public override string Operator => "-";
        public ExpressionBinaryRem(BoxSupportedLexem left, BoxSupportedLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryRem() : base() { }
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetBoxValue(context);
            var right = Right.GetBoxValue(context);
            if (left.Type == SLTSpeedyObject.DoubleType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = left;
                    reference.AsDouble -= right.AsDouble;
                    return ref reference;
                }
                else if (right.Type == SLTSpeedyObject.LongType)
                {
                    reference = left;
                    reference.AsDouble -= right.AsLong;
                    return ref reference;
                }
                else if (right.Type == SLTSpeedyObject.ULongType)
                {
                    reference = left;
                    reference.AsDouble -= right.AsULong;
                    return ref reference;
                }
            }
            else if (left.Type == SLTSpeedyObject.LongType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = left;
                    reference.Type = SLTSpeedyObject.DoubleType;
                    reference.AsDouble = reference.AsLong;
                    reference.AsDouble -= right.AsDouble;
                    return ref reference;
                }
                else if (right.Type == SLTSpeedyObject.LongType)
                {
                    reference = left;
                    reference.AsLong -= right.AsLong;
                    return ref reference;
                }
            }
            else if (left.Type == SLTSpeedyObject.ULongType)
            {
                if (right.Type == SLTSpeedyObject.DoubleType)
                {
                    reference = left;
                    reference.Type = SLTSpeedyObject.DoubleType;
                    reference.AsDouble = reference.AsULong;
                    reference.AsDouble -= right.AsDouble;
                    return ref reference;
                }
                else if (right.Type == SLTSpeedyObject.ULongType)
                {
                    reference = left;
                    reference.AsULong -= right.AsULong;
                    return ref reference;
                }
            }

            throw new UnsupportedTypesInBinaryExpression(this, left.Boxed()?.GetType(), right.Boxed()?.GetType());
        }
    }
}
