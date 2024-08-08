using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Text;

namespace SLThree
{
    public class InterpolatedString : BaseExpression
    {
        public string Value;
        public BaseExpression[] Expressions;

        public InterpolatedString() : base() { }
        public InterpolatedString(string value, IList<(BaseExpression, string)> other, Cursor cursor) : base(cursor)
        {
            var sb = new StringBuilder();
            Expressions = new BaseExpression[other.Count];
            sb.Append(value);
            for (var i = 0; i < other.Count; i++)
            {
                Expressions[i] = other[i].Item1;
                sb.Append($"{{{i}}}");
                sb.Append(other[i].Item2);
            }
            Value = sb.ToString();
        }

        public override string ExpressionToString() => $"$\"{string.Format(Value, Expressions.ConvertAll(x => $"{{{x.ExpressionToString()}}}"))}\"";

        public override object GetValue(ExecutionContext context)
        {
            return string.Format(Value, Expressions.ConvertAll(x => x.GetValue(context)));
        }

        public override object Clone()
        {
            return new InterpolatedString() { Value = Value.CloneCast(), Expressions = Expressions.CloneArray(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
