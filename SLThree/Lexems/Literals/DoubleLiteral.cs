using Pegasus.Common;
using SLThree.Extensions.Cloning;
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
        public override object Clone() => new DoubleLiteral() { Value = Value, SourceContext = SourceContext.CloneCast() };
    }
}
