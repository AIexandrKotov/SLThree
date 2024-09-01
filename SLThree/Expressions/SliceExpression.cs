using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public sealed class SliceExpression : BaseExpression
    {
        public BaseExpression Left;
        public BaseExpression LowerBound;
        public BaseExpression UpperBound;
        public bool Excluding;

        public SliceExpression(BaseExpression left, BaseExpression lowerBound, BaseExpression upperBound, bool excluding, SourceContext context) : base(context)
        {
            Left = left;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Excluding = UpperBound != null && excluding;
        }

        public override string ExpressionToString() => $"{Left}[{LowerBound?.ToString()??""}..{(Excluding?"":"=")}{UpperBound?.ToString()??""}]";

        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);
            var lower = LowerBound?.GetValue(context).CastToType<long>() ?? 0;
            long upper;
            if (left is IEnumerable enumerable)
            {
                var uppercoalesce = UpperBound?.GetValue(context).CastToType<long>();
                if (left is ICollection collection) 
                    upper = uppercoalesce ?? collection.Count;
                else
                    upper = uppercoalesce ?? enumerable.Enumerate().Count();
            }
            else throw new RuntimeError($"{left?.GetType().GetTypeString()} is not enumerable", SourceContext);

            return Slice(left.Cast<IEnumerable>(), (int)lower, (int)(upper - lower - (Excluding ? 1 : 0)));
        }

        public static IEnumerable<object> Slice(IEnumerable enumerable, int skip, int take)
        {
            var enumerator = enumerable.GetEnumerator();
            var i = skip;
            while (i-- > 0 && enumerator.MoveNext()) ;
            i = take;
            while (i-- >= 0 && enumerator.MoveNext())
                yield return enumerator.Current;
        }

        public override object Clone()
        {
            return new SliceExpression(Left.CloneCast(), LowerBound.CloneCast(), UpperBound.CloneCast(), Excluding, SourceContext.CloneCast());
        }
    }
}
