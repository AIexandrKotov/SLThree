using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class FloatLiteral : BoxLiteral<float>
    {
        public FloatLiteral(float value, Cursor cursor) : base(value, cursor) { }
        public FloatLiteral() : base() { }
        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetAny(Value);
            return ref reference;
        }
    }
}
