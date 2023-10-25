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

        public override object GetValue(ExecutionContext context)
        {
            return context.LocalVariables.TryGetValue(Name, out var value) ? value : null;
        }
    }
}
