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
            var cond = Condition.GetValue(context);
            while (cond.Type == SLTSpeedyObject.BoolType && cond.AsBool)
            {
                ret = CycleBody.GetValue(context);
                cond = Condition.GetValue(context);
            }
            return ret;
        }
    }
}
