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
        public StatementListStatement CycleBody { get; set; }

        public WhileCycleStatement(BaseLexem condition, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
        {
            Condition = condition;
            CycleBody = cycleBody;
        }

        public override string ToString() => $"while ({Condition}) {{{CycleBody}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            context.StartCycle();
            while (Condition.GetValue(context).Cast<bool>())
            {
                for (var i = 0; i < CycleBody.Statements.Count; i++)
                {
                    ret = CycleBody.Statements[i].GetValue(context);
                    if (context.Returned || context.Broken) break;
                    if (context.Continued) continue;
                }
            }
            context.EndCycle();
            return ret;
        }
    }
}
