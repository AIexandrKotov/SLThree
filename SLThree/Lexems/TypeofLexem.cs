using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
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

        public TypeofLexem() : base() { }
        public TypeofLexem(BaseLexem type, SourceContext context) : base(context)
        {
            Typename = type;
            if (this.type == null) this.type = Typename.ToString().Replace(" ", "").ToType();
        }

        public override string ToString() => $"{Typename}";

        private Type type;
        public override object GetValue(ExecutionContext context)
        {
            return type;
        }

        public override object Clone()
        {
            return new TypeofLexem() { Typename = Typename.CloneCast(), SourceContext = SourceContext.CloneCast(), type = type };
        }
    }
}
