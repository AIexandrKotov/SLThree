using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class BoxSupportedLexem : BaseLexem
    {

        public BoxSupportedLexem(Cursor cursor) : base(cursor)
        {

        }
        public BoxSupportedLexem() : base(default) { }

        protected SLTSpeedyObject reference;
        public sealed override SLTSpeedyObject GetValue(ExecutionContext context)
        {
            return GetBoxValue(context);
        }

        public abstract ref SLTSpeedyObject GetBoxValue(ExecutionContext context);
    }
}
