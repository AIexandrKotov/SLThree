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

        private bool counted;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            if (counted)
            {
                return context.LocalVariables.GetValue(variable_index);
            }
            else
            {
                var value = default(object);
                (value, variable_index) = context.LocalVariables.GetValue(Name);
                counted = true;
                return value;
            }
        }
    }
}
