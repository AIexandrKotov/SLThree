using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class ConditionStatement : BaseStatement
    {
        public BaseLexem Condition { get; set; }
        public BaseStatement[] Body { get; set; }

        public ConditionStatement() { }
        public ConditionStatement(BaseLexem condition, StatementListStatement trueBlock, StatementListStatement falseBlock, Cursor cursor)
            : this(condition, trueBlock, falseBlock, new SourceContext(cursor)) { }

        public ConditionStatement(BaseLexem condition, StatementListStatement trueBlock, StatementListStatement falseBlock, SourceContext context) : base(context)
        {
            Condition = condition;
            count = trueBlock.Statements.Length + falseBlock.Statements.Length;
            Body = new BaseStatement[count];
            trueBlock.Statements.CopyTo(Body, 0);
            falsestart = trueBlock.Statements.Length;
            falseBlock.Statements.CopyTo(Body, falsestart);
        }
        private int count;
        private int falsestart;

        public override string ToString() => $"if ({Condition}) {{{Body}}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            var cond = (bool)Condition.GetValue(context);
            var start = cond ? 0 : falsestart;
            var end = cond ? falsestart : count;
            for (var i = start; i < end; i++)
            {
                ret = Body[i].GetValue(context);
                if (context.Returned || context.Broken || context.Continued) break;
            }
            return ret;
        }

        public override object Clone()
        {
            return new ConditionStatement()
            {
                Condition = Condition.CloneCast(),
                Body = Body.CloneArray(),
                SourceContext = SourceContext.CloneCast(),
                count = count,
                falsestart = falsestart
            };
        }
    }
}
