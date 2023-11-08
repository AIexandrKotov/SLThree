using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Linq
{
    public static class Linq
    {
        internal static MemberAccess.ClassAccess LinqAccess = new MemberAccess.ClassAccess(typeof(Linq));
#pragma warning disable IDE1006 // Стили именования
        public static IEnumerable<object> range(long end)
        {
            for (var i = 0; i < end; i += 1)
                yield return i;
        }
        public static IEnumerable<object> range(long start, long end)
        {
            for (var i = start; i < end; i += 1)
                yield return i;
        }
        public static IEnumerable<object> range(long start, long end, long step)
        {
            for (var i = start; i < end; i += step)
                yield return i;
        }

        public static object max(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Max(x => method.GetValue(context, new object[] { x }));
        }
        public static object max(IEnumerable<object> objects, Method method)
        {
            return objects.Max(x => method.GetValue(new object[] { x }));
        }
        public static object max(IEnumerable<object> objects) => objects.Max();

        public static object min(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Min(x => method.GetValue(context, new object[] { x }));
        }
        public static object min(IEnumerable<object> objects, Method method)
        {
            return objects.Min(x => method.GetValue(new object[] { x }));
        }
        public static object min(IEnumerable<object> objects) => objects.Min();

        public static IEnumerable<object> map(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Select(x => method.GetValue(context, new object[] { x }));
        }
        public static IEnumerable<object> map(IEnumerable<object> objects, Method method)
        {
            return objects.Select(x => method.GetValue(new object[] { x }));
        }

        public static IEnumerable<object> filter(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Where(x => method.GetValue(context, new object[] { x }).Cast<bool>());
        }
        public static IEnumerable<object> filter(IEnumerable<object> objects, Method method)
        {
            return objects.Where(x => method.GetValue(new object[] { x }).Cast<bool>());
        }

        public static List<object> tolist(IEnumerable<object> objects) => objects.ToList();
        public static object[] toarray(IEnumerable<object> objects) => objects.ToArray();

        public static string jts(IEnumerable<object> objects, string str) => objects.JoinIntoString(str);
        public static string jts(IEnumerable<object> objects) => objects.JoinIntoString(" ");
#pragma warning restore IDE1006 // Стили именования
    }
}
