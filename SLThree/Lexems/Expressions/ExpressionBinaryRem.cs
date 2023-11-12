using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
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
        public ExpressionBinaryRem(BaseLexem left, BaseLexem right, SourceContext context) : base(left, right, context) { }
        public ExpressionBinaryRem() : base() { }
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
                if (right is long i2) return i1 - i2;
                if (right is double d2) return i1 - d2;
            }
            else if (left is double d1)
            {
                if (right is double d2) return d1 - d2;
                if (right is long i2) return d1 - i2;
                if (right is ulong u2) return d1 - u2;
            }
            else if (left is ulong u1)
            {
                if (right is ulong u2) return u1 - u2;
                if (right is double d2) return u1 - d2;
            }
            context.Errors.Add(new OperatorError(this, left?.GetType(), right?.GetType()));
            return null;
        }

        public override object Clone()
        {
            return new ExpressionBinaryRem(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
    }
}
