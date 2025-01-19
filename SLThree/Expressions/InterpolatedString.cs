
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
        public string[] RawRepresentation;

        public InterpolatedString() : base() { }
        public InterpolatedString(string value, IList<(BaseExpression, string)> other, ISourceContext context) : base(context)
        {
            var sb = new StringBuilder();
            Expressions = new BaseExpression[other.Count];
            RawRepresentation = new string[other.Count + 1];
            RawRepresentation[0] = value;
            sb.Append(value);
            for (var i = 0; i < other.Count; i++)
            {
                Expressions[i] = other[i].Item1;
                RawRepresentation[i + 1] = other[i].Item2;
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
            return new InterpolatedString() { Value = Value.CloneCast(), Expressions = Expressions.CloneArray(), RawRepresentation = RawRepresentation.CloneArray(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
