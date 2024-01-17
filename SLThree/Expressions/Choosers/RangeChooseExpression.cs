using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class RangeChooseExpression : BaseExpression, IChooserExpression
    {
        public static readonly Random Random = new Random();

        public BaseExpression LowerBound;
        public BaseExpression UpperBound;

        public RangeChooseExpression(BaseExpression lower, BaseExpression upper, SourceContext context) : base(context)
        {
            LowerBound = lower;
            UpperBound = upper;
        }

        public override string ExpressionToString() => $"{LowerBound}..{UpperBound}";

        private RangeChooser rc = new RangeChooser(0, 1);

        public override object GetValue(ExecutionContext context)
        {
            if (context.fimp)
            {
                rc.LowerBound = LowerBound.GetValue(context).Cast<long>();
                rc.UpperBound = UpperBound.GetValue(context).Cast<long>();
            }
            else
            {
                rc.LowerBound = LowerBound.GetValue(context).CastToMax().Cast<long>();
                rc.UpperBound = UpperBound.GetValue(context).CastToMax().Cast<long>();
            }
            return rc.Choose();
        }

        public override object Clone()
        {
            return new RangeChooseExpression(LowerBound.CloneCast(), UpperBound.CloneCast(), SourceContext.CloneCast());
        }

        public object GetChooser(ExecutionContext context)
        {
            if (context.fimp)
            {
                return new RangeChooser(LowerBound.GetValue(context).Cast<long>(), UpperBound.GetValue(context).Cast<long>());
            }
            else
            {
                return new RangeChooser(LowerBound.GetValue(context).CastToMax().Cast<long>(), UpperBound.GetValue(context).CastToMax().Cast<long>());
            }
        }
    }
}
