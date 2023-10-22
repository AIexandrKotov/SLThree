using Pegasus.Common;
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
        public IList<BaseStatement> Statements;

        public StatementListStatement(IList<BaseStatement> statements, Cursor cursor) : base(cursor)
        {
            Statements = statements;
        }

        public override string ToString() => $"{Statements.Count} statements";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            foreach (var st in Statements)
                ret = st.GetValue(context);
            return ret;
        }
    }
}
