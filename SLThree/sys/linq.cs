using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class linq
    {
        internal static MemberAccess.ClassAccess LinqAccess = new MemberAccess.ClassAccess(typeof(linq));
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

        public static long sum_i64(IEnumerable<object> objects)
            => ExecutionContext.global.pred.fimp
            ? objects.Sum(x => (long)x)
            : objects.Sum(x => (long)x.CastToType(typeof(long)));
        public static long sum_i64(IEnumerable<object> objects, Method method)
            => ExecutionContext.global.pred.fimp
            ? objects.Sum(x => (long)method.GetValue(new object[1] { x }))
            : objects.Sum(x => (long)method.GetValue(new object[1] { x }).CastToType(typeof(long)));
        public static long sum_i64(IEnumerable<object> objects, Method method, ExecutionContext context)
            => context.fimp
            ? objects.Sum(x => (long)method.GetValue(context, new object[1] { x }))
            : objects.Sum(x => (long)method.GetValue(context, new object[1] { x }).CastToType(typeof(long)));
        public static double sum_f64(IEnumerable<object> objects)
            => ExecutionContext.global.pred.fimp
            ? objects.Sum(x => (double)x)
            : objects.Sum(x => (double)x.CastToType(typeof(double)));
        public static double sum_f64(IEnumerable<object> objects, Method method)
            => ExecutionContext.global.pred.fimp
            ? objects.Sum(x => (double)method.GetValue(new object[1] { x }))
            : objects.Sum(x => (double)method.GetValue(new object[1] { x }).CastToType(typeof(double)));
        public static double sum_f64(IEnumerable<object> objects, Method method, ExecutionContext context)
            => context.fimp
            ? objects.Sum(x => (double)method.GetValue(context, new object[1] { x }))
            : objects.Sum(x => (double)method.GetValue(context, new object[1] { x }).CastToType(typeof(double)));
        public static double sum(IEnumerable<object> objects) => sum_f64(objects);
        public static double sum(IEnumerable<object> objects, Method method) => sum_f64(objects, method);
        public static double sum(IEnumerable<object> objects, Method method, ExecutionContext context) => sum_f64(objects, method, context);

        public static double average(IEnumerable<object> objects)
            => ExecutionContext.global.pred.fimp
            ? objects.Average(x => (double)x)
            : objects.Average(x => (double)x.CastToType(typeof(double)));
        public static double average(IEnumerable<object> objects, Method method)
            => ExecutionContext.global.pred.fimp
            ? objects.Average(x => (double)method.GetValue(new object[1] { x }))
            : objects.Average(x => (double)method.GetValue(new object[1] { x }).CastToType(typeof(double)));
        public static double average(IEnumerable<object> objects, Method method, ExecutionContext context)
            => context.fimp
            ? objects.Average(x => (double)method.GetValue(context, new object[1] { x }))
            : objects.Average(x => (double)method.GetValue(context, new object[1] { x }).CastToType(typeof(double)));

        public static object aggregate(IEnumerable<object> objects, Method method)
        {
            return objects.Aggregate((x, y) => method.GetValue(new object[2] { x, y }));
        }
        public static object aggregate(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Aggregate((x, y) => method.GetValue(context, new object[2] { x, y }));
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

        public static object max_by(IEnumerable<object> objects, Method method) 
            => objects.MaxBy(x => method.GetValue(new object[1] { x }));
        public static object max_by(IEnumerable<object> objects, Method method, ExecutionContext context) 
            => objects.MaxBy(x => method.GetValue(context, new object[1] { x }));

        public static object min_by(IEnumerable<object> objects, Method method)
            => objects.MinBy(x => method.GetValue(new object[1] { x }));
        public static object min_by(IEnumerable<object> objects, Method method, ExecutionContext context)
            => objects.MinBy(x => method.GetValue(context, new object[1] { x }));

        public static IEnumerable<object> select(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Select(x => method.GetValue(context, new object[] { x }));
        }
        public static IEnumerable<object> select(IEnumerable<object> objects, Method method)
        {
            return objects.Select(x => method.GetValue(new object[] { x }));
        }

        public static IEnumerable<object> select_many(IEnumerable<object> objects) => objects.Select(x => (IEnumerable<object>)x).SelectMany(x => x);
        public static IEnumerable<object> select_many(IEnumerable<object> objects, Method method)
        {
            return objects.Select(x => (IEnumerable<object>)x).SelectMany(x => method.GetValue(new object[1] { x }) as IEnumerable<object>);
        }
        public static IEnumerable<object> select_many(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Select(x => (IEnumerable<object>)x).SelectMany(x => method.GetValue(context, new object[1] { x }) as IEnumerable<object>);
        }

        public static bool any(IEnumerable<object> objects) => objects.Any();
        public static bool any(IEnumerable<object> objects, Method method)
        {
            return objects.Any(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static bool any(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Any(x => (bool)method.GetValue(context, new object[1] { x }));
        }

        public static bool all(IEnumerable<object> objects, Method method)
        {
            return objects.All(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static bool all(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.All(x => (bool)method.GetValue(context, new object[1] { x }));
        }

        public static IEnumerable<object> where(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Where(x => method.GetValue(context, new object[] { x }).Cast<bool>());
        }
        public static IEnumerable<object> where(IEnumerable<object> objects, Method method)
        {
            return objects.Where(x => method.GetValue(new object[] { x }).Cast<bool>());
        }

        public static long count(IEnumerable<object> objects) => objects.Count();
        public static long count(IEnumerable<object> objects, Method method)
        {
            return objects.Count(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static long count(IEnumerable<object> objects, Method method, ExecutionContext context)
        {
            return objects.Count(x => (bool)method.GetValue(context, new object[1] { x }));
        }

        public static IEnumerable<object> entuple(ITuple tuple) => tuple.Enumerate();
        public static IEnumerable<object> enumerate(IEnumerable<object> objects)
        {
            var i = 0;
            foreach(var x in objects)
            {
                yield return (i, x);
                i++;
            }
        }

        public static Dictionary<object, object> to_dict(IEnumerable<object> objects, Method methodKey, Method methodValue)
            => objects.ToDictionary(x => methodKey.GetValue(new object[1] { x }), x => methodValue.GetValue(new object[1] { x }));
        public static Dictionary<object, object> to_dict(IEnumerable<object> objects, Method methodKey, Method methodValue, ExecutionContext context)
            => objects.ToDictionary(x => methodKey.GetValue(context, new object[1] { x }), x => methodValue.GetValue(context, new object[1] { x }));
        public static List<object> to_list(IEnumerable<object> objects) => objects.ToList();
        public static ITuple to_tuple(IEnumerable<object> objects) => CreatorTuple.Create(objects.ToArray());
        public static object[] to_array(IEnumerable<object> objects) => objects.ToArray();

        public static IEnumerable<object> skip(IEnumerable<object> objects, long count) => objects.Skip((int)count);
        public static IEnumerable<object> take(IEnumerable<object> objects, long count) => objects.Take((int)count);

        public static string jts(IEnumerable<object> objects, string str) => objects.JoinIntoString(str);
        public static string jts(IEnumerable<object> objects) => objects.JoinIntoString(" ");
    }
#pragma warning restore IDE1006 // Стили именования
}
