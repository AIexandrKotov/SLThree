using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class Special : BaseExpression
    {
        public Special()
        {
        }

        public Special(SourceContext context) : base(context)
        {
        }

        public Special(bool priority, SourceContext context) : base(priority, context)
        {
        }
    }
}
