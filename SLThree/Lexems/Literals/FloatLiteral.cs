using Pegasus.Common;
using SLThree.Extensions.Cloning;
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
        public override object Clone() => new FloatLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
