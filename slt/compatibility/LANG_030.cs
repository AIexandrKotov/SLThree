using slt.sys;
using SLThree;
using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static slt.Program;

namespace slt
{
    public class LANG_030
    {
        public static bool Supports { get; internal set; }

        public static object SafeFromContext(object value)
        {
            if (value is ExecutionContext.ContextWrap wrap)
                return $"context {wrap.pred.Name}";
            else return value;
        }

        public static object GetChoosersOutput(object value)
        {
            if (value == null) return value;
            if (value is ITuple tuple)
            {
                var xvalue = CreatorTuple.ToArray(tuple); value =
                    $"({(xvalue.Length <= repl.count ? xvalue.Enumerate().Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") : xvalue.Enumerate().Take(repl.count).Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") + "...")})";
            }
            if (value is IList list) value =
                    $"[{(list.Count <= repl.count ? list.Enumerate().Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") : list.Enumerate().Take(repl.count).Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") + "...")}]";
            if (value is IDictionary dict) value =
                    $"{{{(dict.Count <= repl.count ? dict.Keys.Enumerate().Select(x => $"{SafeFromContext(GetOutput(x))}: {SafeFromContext(GetOutput(dict[x]))}").JoinIntoString(", ") : dict.Keys.Enumerate().Take(repl.count).Select(x => $"{SafeFromContext(GetOutput(x))}: {SafeFromContext(GetOutput(dict[x]))}").JoinIntoString(", ") + "...")}}}";
            return value;
        }
    }
}
