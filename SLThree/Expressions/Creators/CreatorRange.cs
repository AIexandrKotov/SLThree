using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.sys;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class CreatorRange : BaseExpression
    {
        public BaseExpression LowerBound;
        public BaseExpression UpperBound;

        public CreatorRange(BaseExpression lowerBound, BaseExpression upperBound, SourceContext context) : base(context)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public struct RangeEnumerator : IEnumerable<object>
        {
            long lower;
            long upper;

            public RangeEnumerator(long lower, long upper)
            {
                this.lower = lower;
                this.upper = upper;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<object> GetEnumerator()
            {
                for (var i = lower; i <= upper; i++)
                    yield return i;
            }

            public override string ToString() => $"{lower}..{upper}";
        }

        public override object GetValue(ExecutionContext context)
        {
            long lower;
            long upper;
            if (context.fimp)
            {
                lower = LowerBound.GetValue(context).Cast<long>();
                upper = UpperBound.GetValue(context).Cast<long>();
            }
            else
            {
                lower = LowerBound.GetValue(context).CastToMax().Cast<long>();
                upper = UpperBound.GetValue(context).CastToMax().Cast<long>();
            }
            return new RangeEnumerator(lower, upper);
        }

        public override string ExpressionToString() => $"{LowerBound}..{UpperBound}";

        public override object Clone()
        {
            return new CreatorRange(LowerBound.CloneCast(), UpperBound.CloneCast(), SourceContext.CloneCast());
        }
    }
}
