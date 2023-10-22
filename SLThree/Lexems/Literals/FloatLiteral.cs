using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class FloatLiteral : Literal<float>
    {
        public FloatLiteral(float value, Cursor cursor) : base(value, cursor) { }
        public FloatLiteral() : base() { }
    }
}
