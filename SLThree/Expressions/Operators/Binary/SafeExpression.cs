using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class SafeExpression : BinaryOperator
    {
        public override string Operator => "-?";
        public SafeExpression(BaseExpression left, BaseExpression right, SourceContext context, bool priority = false) : base(left, right, context, priority) { }
        public SafeExpression() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            try
            {
                return Left.GetValue(context);
            }
            catch
            {
                return Right.GetValue(context);
            }
        }

        public override object Clone()
        {
            return new SafeExpression(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
