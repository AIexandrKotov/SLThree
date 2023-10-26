using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class TypeofLexem : BoxSupportedLexem
    {
        public BaseLexem Typename;

        public TypeofLexem(BaseLexem type, Cursor cursor) : base(cursor)
        {
            Typename = type;
        }

        public override string ToString() => $"{Typename}";

        private Type type;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            if (type != null)
            {
                return ref type.ToSpeedy(ref reference);
            }
            else
            {
                var x = Typename.ToString().Replace(" ", "");
                type = x.ToType();
                return ref type.ToSpeedy(ref reference);
            }
        }
    }
}
