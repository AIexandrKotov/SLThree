using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class InfiniteLoopStatement : BaseLoopStatement
    {
        public InfiniteLoopStatement() : base() { }
        public InfiniteLoopStatement(StatementList cycleBody, ISourceContext context) : base(cycleBody, context)
        {
        }
        public InfiniteLoopStatement(BaseStatement[] cycleBody, ISourceContext context) : base(cycleBody, context)
        {
        }

        public override string ToString() => $"loop {{{LoopBody}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            while (true)
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
            return new InfiniteLoopStatement(LoopBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
