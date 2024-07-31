using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class ConditionExpression : BaseExpression
    {
        public BaseExpression Condition;
        public BaseStatement[] Body;

        public ConditionExpression() { }
        public ConditionExpression(BaseExpression condition, BaseStatement[] body, int falsestart, SourceContext context) : base(context)
        {
            Condition = condition;
            Body = body;
            count = Body.Length;
            this.falsestart = falsestart;
        }

        public ConditionExpression(BaseExpression condition, StatementList trueBlock, StatementList falseBlock, SourceContext context) : base(context)
        {
            Condition = condition;
            count = trueBlock.Statements.Length + falseBlock.Statements.Length;
            Body = new BaseStatement[count];
            trueBlock.Statements.CopyTo(Body, 0);
            falsestart = trueBlock.Statements.Length;
            falseBlock.Statements.CopyTo(Body, falsestart);
        }
        private int count;
        private int falsestart;

        public BaseStatement[] IfBody => Body.Take(falsestart).ToArray();
        public BaseStatement[] ElseBody => Body.Skip(falsestart).ToArray();

        public override string ExpressionToString() => $"if ({Condition}) {{{Body}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            var cond = (bool)Condition.GetValue(context);
            var start = cond ? 0 : falsestart;
            var end = cond ? falsestart : count;
            for (var i = start; i < end; i++)
            {
                ret = Body[i].GetValue(context);
                if (context.Returned || context.Broken || context.Continued) break;
            }
            return ret;
        }

        public override object Clone()
        {
            return new ConditionExpression(Condition.CloneCast(), Body.CloneArray(), falsestart, SourceContext.CloneCast());
        }
    }
}
