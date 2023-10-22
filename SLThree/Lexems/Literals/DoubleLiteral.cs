using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class DoubleLiteral : Literal<double>
    {
        public DoubleLiteral(double value, Cursor cursor) : base(value, cursor) { }
        public DoubleLiteral() : base() { }
    }
}
