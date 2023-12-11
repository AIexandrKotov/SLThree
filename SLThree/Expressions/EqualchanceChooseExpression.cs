using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class EqualchanceChooseExpression : BaseExpression
    {
        public static readonly Random Random = new Random();

        public EqualchanceChooser<BaseExpression> Chooser { get; set; }

        public EqualchanceChooseExpression(IList<BaseExpression> elements, SourceContext context) : base(context)
        {
            Chooser = new EqualchanceChooser<BaseExpression>(elements);
        }

        public EqualchanceChooser<object> GetChooser(ExecutionContext context)
        {
            return new EqualchanceChooser<object>(Chooser.Values.Select(x => x.GetValue(context)).ToArray());
        }

        public override object GetValue(ExecutionContext context)
        {
            return Chooser.Choose().GetValue(context);
        }

        public override string ExpressionToString() => Chooser.Values.JoinIntoString(" \\ ");

        public override object Clone()
        {
            return new EqualchanceChooseExpression(Chooser.Values.ToArray().CloneArray(), SourceContext);
        }
    }
}
