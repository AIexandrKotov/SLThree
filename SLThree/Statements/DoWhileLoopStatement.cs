using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class DoWhileLoopStatement : BaseLoopStatement
    {
        public BaseExpression Condition;

        public DoWhileLoopStatement() : base() { }
        public DoWhileLoopStatement(BaseExpression condition, StatementList cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Condition = condition;
        }
        public DoWhileLoopStatement(BaseExpression condition, BaseStatement[] cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Condition = condition;
        }

        public override string ToString() => $"do {{{LoopBody}}} while ({Condition})";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            do
            {
                for (var i = 0; i < count; i++)
                {
                    ret = LoopBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
                context.Continued = false;
                if (context.Returned || context.Broken) break;
            }
            while ((bool)Condition.GetValue(context));
            context.EndCycle();
            return ret;
        }

        public override object Clone()
        {
            return new DoWhileLoopStatement(Condition.CloneCast(), LoopBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
