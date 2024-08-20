using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class FiniteLoopStatement : BaseLoopStatement
    {
        public BaseExpression Iterations;

        public FiniteLoopStatement() : base() { }
        public FiniteLoopStatement(BaseExpression condition, StatementList cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Iterations = condition;
        }
        public FiniteLoopStatement(BaseExpression condition, BaseStatement[] cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Iterations = condition;
        }

        public override string ToString() => $"loop ({Iterations}) {{{LoopBody}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            var iters = 0;
            var end = Iterations.GetValue(context).CastToType<long>();
            while (iters++ < end)
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
            return new FiniteLoopStatement(Iterations.CloneCast(), LoopBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
