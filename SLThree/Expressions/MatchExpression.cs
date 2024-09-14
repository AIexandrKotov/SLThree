using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLThree
{
    public class MatchExpression : BaseExpression
    {
        public BaseExpression Matching;
        public BaseExpression[][] Matches;
        public BaseStatement[] Cases;
        public BaseStatement InDefault;
        private int count;

        public MatchExpression(BaseExpression matching, IList<(IList<BaseExpression>, BaseStatement)> matches, ISourceContext context) : base(context)
        {
            Matching = matching;
            var m = matches.Select(x => x.Item1?.ToArray()).ToList();
            var c = matches.Select(x => x.Item2).ToList();
            var found = false;
            for (var i = 0; i < m.Count; i++)
            {
                if (m[i] == null)
                {
                    if (found) throw new LogicalError("Match expression should have only one empty case", c[i].SourceContext);
                    found = true;
                    InDefault = c[i];
                    m.RemoveAt(i);
                    c.RemoveAt(i);
                    i--;
                }
            }
            Matches = m.ToArray();
            Cases = c.ToArray();
            count = Matches.Length;
        }
        public MatchExpression(BaseExpression matching, BaseExpression[][] matches, BaseStatement[] cases, BaseStatement inDefault, ISourceContext context) : base(context)
        {
            Matching = matching;
            Matches = matches;
            Cases = cases;
            InDefault = inDefault;
            count = Matches.Length;
        }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"match {Matching} {{");
            for (var i = 0; i < count; i++)
                sb.AppendLine($"{Matches[i]} => {Cases[i]}");
            sb.AppendLine("}");
            return sb.ToString();
        }
        public override object GetValue(ExecutionContext context)
        {
            var value = Matching.GetValue(context);
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < Matches[i].Length; j++)
                {
                    var right = Matches[i][j].GetValue(context);
                    if (value != null && value.Equals(right))
                        return Cases[i].GetValue(context);
                    if (right != null && right.Equals(value))
                        return Cases[i].GetValue(context);
                    if (value == right)
                        return Cases[i].GetValue(context);
                }
            }
            if (InDefault != null)
                return InDefault.GetValue(context);
            return null;
        }
        public override object Clone()
        {
            return new MatchExpression(Matching.CloneCast(), Matches.ConvertAll(x => x.CloneArray()), Cases.CloneArray(), InDefault.CloneCast(), SourceContext.CloneCast());
        }
    }
}
