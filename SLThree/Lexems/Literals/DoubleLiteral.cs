using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class DoubleLiteral : BoxLiteral<double>
    {
        public DoubleLiteral(double value, Cursor cursor) : base(value, cursor) { }
        public DoubleLiteral() : base() { }
        private SLTSpeedyObject reference;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            reference = SLTSpeedyObject.GetDouble(Value);
            return ref reference;
        }
    }
}
