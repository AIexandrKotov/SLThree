using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class WhileCycleStatement : BaseStatement
    {
        public BaseLexem Condition { get; set; }
        public BaseStatement[] CycleBody { get; set; }

        public WhileCycleStatement(BaseLexem condition, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
        {
            Condition = condition;
            CycleBody = cycleBody.Statements.ToArray();
            count = CycleBody.Length;
        }

        public override string ToString() => $"while ({Condition}) {{{CycleBody}}}";

        private int count;
        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            while ((bool)Condition.GetValue(context))
            {
                for (var i = 0; i < count; i++)
                {
                    ret = CycleBody[i].GetValue(context);
                    if (context.Returned || context.Broken) break;
                    if (context.Continued) continue;
                }
            }
            context.EndCycle();
            return ret;
        }
    }
}
