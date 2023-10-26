using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SLThree
{
    public partial class NameLexem : BaseLexem
    {
        public string Name;
        public NameLexem(string name, Cursor cursor) : base(cursor)
        {
            Name = name;
        }
        public NameLexem(string name, string next, Cursor cursor) : base(cursor)
        {
            Name = name;
            if (next.Length != 0) Name += $".{next}";
        }

        public override string ToString() => Name;

        private ExecutionContext counted;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            if (counted == context)
            {
                return context.LocalVariables.GetValue(variable_index);
            }
            else
            {
                var (value, ind) = context.LocalVariables.GetValue(Name);
                counted = context;
                variable_index = ind;
                return value;
            }
        }
    }
}
