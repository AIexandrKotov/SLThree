using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class StatementListStatement : BaseStatement
    {
        public BaseStatement[] Statements;
        private int count;

        public StatementListStatement() : base() { }

        public StatementListStatement(IList<BaseStatement> statements, SourceContext context) : base(context)
        {
            Statements = statements.ToArray();
            count = statements.Count;
        }

        public override string ToString() => $"{Statements.Length} statements";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            for (var i = 0; i < count; i++)
            {
                ret = Statements[i].GetValue(context);
                if (context.Returned || context.Broken || context.Continued) break;
            }
            return ret;
        }

        public override object Clone()
        {
            return new StatementListStatement() { Statements = Statements.CloneArray(), count = count.Copy(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
