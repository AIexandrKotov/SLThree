using Pegasus.Common;
using SLThree.Extensions;
using System;

namespace SLThree
{
    public class ExpressionBinaryEquals : ExpressionBinary
    {
        public override string Operator => "==";
        public ExpressionBinaryEquals(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryEquals() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            object right;
            if (context.fimp)
            {
                left = Left.GetValue(context);
                right = Right.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
                right = Right.GetValue(context).CastToMax();
            }
            if (left is long i1)
            {
                if (right is long i2) return i1 == i2;
                if (right is double d2) return i1 == d2;
            }
            else if (left is double d1)
            {
                if (right is double d2) return d1 == d2;
                if (right is long i2) return d1 == i2;
                if (right is ulong u2) return d1 == u2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 == u2;
                if (right is double d2) return u1 == d2;
            }
            if (!context.fimp)
                return (left as IComparable).CompareTo(right) == 0;
            context.Errors.Add(new OperatorError(this, left?.GetType(), right?.GetType()));
            return null;
        }
    }
}
