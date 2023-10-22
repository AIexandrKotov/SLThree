﻿using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionBinaryLessThanEquals : ExpressionBinary
    {
        public override string Operator => "<=";
        public ExpressionBinaryLessThanEquals(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryLessThanEquals() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).CastToMax();
            var right = Right.GetValue(context).CastToMax();
            if (left is long i1)
            {
                if (right is double d2) return i1 <= d2;
                if (right is long i2) return i1 <= i2;
            }
            else if (left is double d1)
            {
                if (right is double d2) return d1 <= d2;
                if (right is long i2) return d1 <= i2;
                if (right is ulong u2) return d1 <= u2;
            }
            else if (left is ulong u1)
            {
                if (right is double d2) return u1 <= d2;
                if (right is ulong u2) return u1 <= u2;
            }
            throw new UnsupportedTypesInBinaryExpression(this, left?.GetType(), right?.GetType());
        }
    }
}
