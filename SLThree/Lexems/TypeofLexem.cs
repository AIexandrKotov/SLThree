using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class TypeofLexem : BaseLexem
    {
        public BaseLexem Typename;

        public TypeofLexem(BaseLexem type, Cursor cursor) : base(cursor)
        {
            Typename = type;
        }

        public override string ToString() => $"{Typename}";

        public override object GetValue(ExecutionContext context)
        {
            var x = Typename.ToString().Replace(" ", "");
            return x.ToType();
        }
    }
}
