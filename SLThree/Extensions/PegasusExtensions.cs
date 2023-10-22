using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Pegasus.Common;

namespace SLThree.Extensions
{
    public static class PegasusExtensions
    {
        public static SourceContext ToSC(this Cursor cursor)
        {
            return new SourceContext(cursor);
        }
    }
}
