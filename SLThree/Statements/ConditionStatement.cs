using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ConditionStatement : BaseStatement
    {
        public BaseExpression Condition { get; set; }
        public BaseStatement[] Body { get; set; }

        public ConditionStatement() { }
        public ConditionStatement(BaseExpression condition, BaseStatement[] body, int falsestart, SourceContext context) : base(context)
        {
            Condition = condition;
            Body = body;
            count = Body.Length;
            this.falsestart = falsestart;
        }

        public ConditionStatement(BaseExpression condition, StatementListStatement trueBlock, StatementListStatement falseBlock, SourceContext context) : base(context)
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

        public override string ToString() => $"if ({Condition}) {{{Body}}}";

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
            return new ConditionStatement(Condition.CloneCast(), Body.CloneArray(), falsestart, SourceContext.CloneCast());
        }
    }
}
