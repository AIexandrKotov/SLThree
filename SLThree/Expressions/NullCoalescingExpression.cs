using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    internal class NullCoalescingExpression : ExpressionBinary
    {
        public override string Operator => "??";
        public NullCoalescingExpression(BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public NullCoalescingExpression() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);
            if (left == null) return Right.GetValue(context);
            return left;
        }

        public override object Clone()
        {
            return new NullCoalescingExpression(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
