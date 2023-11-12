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

        public static object GetChoosersOutput(object value)
        {
            if (value == null) return value;
            if (value is ITuple tuple)
            {
                var xvalue = CreatorTuple.ToArray(tuple); value =
                    $"({(xvalue.Length <= 10 ? xvalue.Enumerate().Select(x => GetOutput(x)).JoinIntoString(", ") : xvalue.Enumerate().Take(10).Select(x => GetOutput(x)).JoinIntoString(", ") + "...")})";
            }
            if (value is IList list) value =
                    $"[{(list.Count <= 10 ? list.Enumerate().Select(x => GetOutput(x)).JoinIntoString(", ") : list.Enumerate().Take(10).Select(x => GetOutput(x)).JoinIntoString(", ") + "...")}]";
            if (value is IDictionary dict) value =
                    $"{{{(dict.Count <= 10 ? dict.Keys.Enumerate().Select(x => $"{GetOutput(x)}: {GetOutput(dict[x])}").JoinIntoString(", ") : dict.Keys.Enumerate().Take(10).Select(x => $"{GetOutput(x)}: {GetOutput(dict[x])}").JoinIntoString(", ") + "...")}}}";
            return value;
        }
    }
}
