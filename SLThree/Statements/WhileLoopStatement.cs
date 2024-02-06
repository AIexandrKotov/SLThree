using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class WhileLoopStatement : BaseStatement
    {
        public BaseExpression Condition { get; set; }
        public BaseStatement[] LoopBody { get; set; }

        public WhileLoopStatement() : base() { }
        public WhileLoopStatement(BaseExpression condition, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
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
