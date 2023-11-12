﻿using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ExpressionUnaryRem : ExpressionUnary
    {
        public override string Operator => "-";
        public ExpressionUnaryRem(BaseLexem left, SourceContext context) : base(left, context) { }
        public ExpressionUnaryRem() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            object left;
            if (context.fimp)
            {
                left = Left.GetValue(context);
            }
            else
            {
                left = Left.GetValue(context).CastToMax();
            }
            switch (left)
            {
                case long v: return -v;
                case double v: return -v;
            }
            context.Errors.Add(new OperatorError(this, left?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new ExpressionUnaryRem(Left.CloneCast(), SourceContext.CloneCast());
        }
    }
}
