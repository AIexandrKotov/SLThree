using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class WhileLoopStatement : BaseLoopStatement
    {
        public BaseExpression Condition;

        public WhileLoopStatement() : base() { }
        public WhileLoopStatement(BaseExpression condition, StatementList cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Condition = condition;
        }
        public WhileLoopStatement(BaseExpression condition, BaseStatement[] cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Condition = condition;
        }

        public override string ToString() => $"while ({Condition}) {{{LoopBody}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            while ((bool)Condition.GetValue(context))
            {
                for (var i = 0; i < count; i++)
                {
                    ret = LoopBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
                context.Continued = false;
                if (context.Returned || context.Broken) break;
            }
            context.EndCycle();
            return ret;
        }

        public override object Clone()
        {
            return new WhileLoopStatement(Condition.CloneCast(), LoopBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
