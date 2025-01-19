using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SLThree
{
    public class AccordExpression : BaseExpression
    {
        public override int Priority => 10;
        public BaseExpression[] HeadAccords;
        public (BaseExpression, TemplateMethod.ConstraintDefinition)[][] Accordings;
        public BaseStatement[] Cases;
        public BaseStatement InDefault;
        private int count;
        private string[][] targetNames;
        
        private static BaseExpression SafeElementAt((BaseExpression, TemplateMethod.ConstraintDefinition)[] arr, int i)
        {
            if (i < arr.Length) return arr[i].Item1;
            return null;
        }
        private void CountBuffer()
        {
            var bufferSize = Math.Max(HeadAccords.Length, Accordings.Any() ? Accordings.Max(x => x?.Length ?? 0) : 0);
            targetNames = new string[Accordings.Length][];
            var heads = HeadAccords.Select(x => x is NameExpression nameExpression ? nameExpression.Name : "target").ToArray();
            for (var i = 0; i < Accordings.Length; i++)
            {
                targetNames[i] = new string[bufferSize];
                for (var j = 0; j < Accordings[i].Length; j++)
                {
                    var name = SafeElementAt(Accordings[i], j) is NameExpression nameExpression ? nameExpression.Name : null;
                    if (j < HeadAccords.Length)
                        targetNames[i][j] = name ?? heads[j] ?? "target";
                    else targetNames[i][j] = name ?? "target";
                }
            }
        }

        public AccordExpression(BaseExpression[] matching, IList<(IList<(BaseExpression, TemplateMethod.ConstraintDefinition)>, BaseStatement)> matches, ISourceContext context) : base(context)
        {
            HeadAccords = matching;
            var a = matches.Select(x => x.Item1?.ToArray()).ToList();
            var c = matches.Select(x => x.Item2).ToList();
            var found = false;
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i].Length == 0)
                {
                    if (found) throw new LogicalError("Accord expression should have only one empty case", c[i].SourceContext);
                    found = true;
                    InDefault = c[i];
                    a.RemoveAt(i);
                    c.RemoveAt(i);
                    i--;
                }
            }
            Accordings = a.ToArray();
            Cases = c.ToArray();
            count = Accordings.Length;
            CountBuffer();
        }
        internal AccordExpression(BaseExpression[] matching, (BaseExpression, TemplateMethod.ConstraintDefinition)[][] matches, BaseStatement[] cases, BaseStatement inDefault, ISourceContext context) : base(context)
        {
            HeadAccords = matching;
            Accordings = matches;
            Cases = cases;
            InDefault = inDefault;
            count = Accordings.Length;
            CountBuffer();
        }

        public override string ExpressionToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"accord ({HeadAccords.JoinIntoString(", ")}) {{");
            for (var i = 0; i < count; i++)
                sb.AppendLine($"{Accordings[i].Select(x => $"{(x.Item1 == null ? "" : $"{x.Item1}: ")}{x.Item2}")} => {Cases[i]}");
            sb.AppendLine("}");
            return sb.ToString();
        }
        public override object GetValue(ExecutionContext context)
        {
            var heads = HeadAccords.ConvertAll(x => x.GetValue(context));
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < Accordings[i].Length; j++)
                {
                    if (Accordings[i].All(x => x.Item2
                            .GetConstraint(targetNames[i][j], context)
                            .Applicable(TemplateMethod.GenericMaking.AsValue, Accordings[i][j].Item1?.GetValue(context) ?? heads[j])))
                        return Cases[i].GetValue(context);
                }
            }
            if (InDefault != null)
                return InDefault.GetValue(context);
            return null;
        }
        public override object Clone()
        {
            return new AccordExpression(HeadAccords.CloneCast(), Accordings.ConvertAll(x => x.ConvertAll(y => (y.Item1.CloneCast(), y.Item2.CloneCast()))), Cases.CloneArray(), InDefault.CloneCast(), SourceContext.CloneCast());
        }
    }
}
