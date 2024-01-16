using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class BooleanChooseExpression : BaseExpression, IChooserExpression
    {
        public static readonly Random Random = new Random();

        public BaseExpression Limit { get; set; }

        public BooleanChooseExpression(BaseExpression expression, SourceContext context) : base(context)
        {
            Limit = expression;
        }

        public override string ExpressionToString() => $"\\ {Limit} \\";

        private BooleanChooser bc = new BooleanChooser(1.0);

        public override object GetValue(ExecutionContext context)
        {
            bc.Limit = Limit.GetValue(context).Cast<double>();
            return bc.Choose();
        }

        public object GetChooser(ExecutionContext context)
        {
            return new BooleanChooser(Limit.GetValue(context).Cast<double>());
        }

        public override object Clone()
        {
            return new BooleanChooseExpression(Limit.CloneCast(), SourceContext.CloneCast());
        }
    }
}
