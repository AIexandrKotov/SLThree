using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Extensions
{
    public static class GenericExtensions
    {
        public static string JoinIntoString<T>(this IEnumerable<T> e, string delim)
        {
            var g = e.GetEnumerator();
            var sb = new StringBuilder("");
            if (g.MoveNext())
            {
                sb.Append(g.Current.ToString());
                while (g.MoveNext()) sb.Append(delim + g.Current.ToString());
            }
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut[] ConvertAll<TIn, TOut>(this TIn[] array, Converter<TIn, TOut> func)
            => Array.ConvertAll(array, func);

        public static IList<T> AddAndRet<T>(this IList<T> list, T item)
        {
            var ret = new List<T>(list);
            ret.Insert(0, item);
            return ret;
        }
        public static string ReplaceAll(this string str, IDictionary<string, string> replacer)
        {
            foreach (var x in replacer)
                str = str.Replace(x.Key, x.Value);
            return str;
        }

        public static IEnumerable<object> Enumerate(this IEnumerable enumerable)
        {
            foreach (var x in enumerable)
                yield return x;
        }
        public static IEnumerable<object> Enumerate(this ITuple tuple)
        {
            for (var i = 0; i < tuple.Length; i++)
                yield return tuple[i];
        }

        public static TOut Cast<TIn, TOut>(this TIn o) where TOut: TIn => (TOut)o;
        public static T Cast<T>(this object o) => (T)o;
        public static T? TryCast<T>(this object o) where T : struct => o is T ? (T)o : default;
        public static T TryCastRef<T>(this object o) where T : class => o is T ? (T)o : null;

        public static T MinBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var comp = Comparer<TKey>.Default;
            return enumerable.Aggregate((min, x) => comp.Compare(selector(x), selector(min)) < 0 ? x : min);
        }
        public static T MaxBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var comp = Comparer<TKey>.Default;
            return enumerable.Aggregate((max, x) => comp.Compare(selector(x), selector(max)) > 0 ? x : max);
        }

        public static ChanceChooser<TOut> ConvertChooser<TIn, TOut>(this ChanceChooser<TIn> input, Func<TIn, TOut> selector)
            => new ChanceChooser<TOut>(input.Values.Select(x => (selector(x.Item1), x.Item2)).ToArray());
        public static ChanceChooser<TOut> ConvertChooser<TIn, TOut>(this ChanceChooser<TIn> input) where TOut: TIn
            => new ChanceChooser<TOut>(input.Values.Select(x => ((TOut)x.Item1, x.Item2)).ToArray());
    }
}
