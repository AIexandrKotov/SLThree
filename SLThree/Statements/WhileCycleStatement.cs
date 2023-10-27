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
        public BaseStatement CycleBody { get; set; }

        public WhileCycleStatement(BaseLexem condition, BaseStatement cycleBody, Cursor cursor) : base(cursor)
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
                ret = CycleBody.GetValue(context);
                if (context.Returned || context.Broken) return ret;
                if (context.Continued) continue;
            }
            context.EndCycle();
            return ret;
        }
    }
}
