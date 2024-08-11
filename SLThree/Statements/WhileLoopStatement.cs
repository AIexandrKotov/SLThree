using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class WhileLoopStatement : BaseStatement
    {
        public BaseExpression Condition;
        public BaseStatement[] LoopBody;

        public WhileLoopStatement() : base() { }
        public WhileLoopStatement(BaseExpression condition, StatementList cycleBody, SourceContext context) : base(context)
        {
            Condition = condition;
            LoopBody = cycleBody.Statements.ToArray();
            count = LoopBody.Length;
        }

        public override string ToString() => $"while ({Condition}) {{{LoopBody}}}";

        private int count;
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
            return new WhileLoopStatement()
            {
                Condition = Condition.CloneCast(),
                LoopBody = LoopBody.CloneArray(),
                count = count.Copy(),
                SourceContext = SourceContext.CloneCast()
            };
        }
    }
}
