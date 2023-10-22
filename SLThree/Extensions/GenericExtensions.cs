using System;
using System.Collections.Generic;
using System.Linq;
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

        public static T ToEnum<T>(this string s) where T : Enum => (T)Enum.Parse(typeof(T), s);

        public static TOut Cast<TIn, TOut>(this TIn o) where TOut: TIn => (TOut)o;
        public static T Cast<T>(this object o) => (T)o;
    }
}
