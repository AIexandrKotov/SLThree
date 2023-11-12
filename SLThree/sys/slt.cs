using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class slt
    {
        public static string repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ExecutionContext.ContextWrap wrap) => wrap.ToDetailedString(1, new List<ExecutionContext.ContextWrap>());
    }
#pragma warning restore IDE1006 // Стили именования
}
