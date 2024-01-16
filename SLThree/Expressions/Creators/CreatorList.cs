using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{

    public class CreatorList : BaseExpression
    {
        public BaseExpression[] Expressions;

        public CreatorList(BaseExpression[] expressions, SourceContext context) : base(context)
        {
            Expressions = expressions;
        }

        public override object GetValue(ExecutionContext context)
        {
            return Expressions.ConvertAll(x => x.GetValue(context)).ToList();
        }

        public override string ExpressionToString() => $"[{Expressions.JoinIntoString(", ")}]";

        public override object Clone()
        {
            return new CreatorList(Expressions.CloneArray(), SourceContext);
        }
    }
}
