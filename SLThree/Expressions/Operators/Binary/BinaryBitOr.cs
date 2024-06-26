﻿using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BinaryBitOr : BinaryOperator
    {
        public override string Operator => "|";
        public BinaryBitOr(BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }

        public BinaryBitOr() : base() { }


        public override object GetValue(ExecutionContext context)
        {
            object left;
            object right;
            if (context.ForbidImplicit)
            {
                left = Left.GetValue(context);
                right = Right.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
                right = Right.GetValue(context).CastToMax();
            }
            if (left is bool b1 && right is bool b2) return b1 | b2;
            else if (left is long i1)
            {
                if (right is long i2) return i1 | i2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 | u2;
            }
            throw new OperatorError(this, left?.GetType(), right?.GetType());
        }

        public override object Clone()
        {
            return new BinaryBitOr(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
