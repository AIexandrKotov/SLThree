using SLThree.Extensions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class tlinq
    {
        //todo sum, average, group_by, order_by

        private static class sum_helper
        {
            public static Type EnumerableType = typeof(Enumerable);
            public static Dictionary<Type, Delegate> methods = new Dictionary<Type, Delegate>()
            {
                { typeof(int), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<int>) }).CreateDelegate(typeof(Func<IEnumerable<int>, int>)) },
                { typeof(long), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<long>) }).CreateDelegate(typeof(Func<IEnumerable<long>, long>)) },
                { typeof(float), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<float>) }).CreateDelegate(typeof(Func<IEnumerable<float>, float>)) },
                { typeof(double), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<double>) }).CreateDelegate(typeof(Func<IEnumerable<double>, double>)) },
                { typeof(decimal), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<decimal>) }).CreateDelegate(typeof(Func<IEnumerable<decimal>, decimal>)) },
                { typeof(int?), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<int?>) }).CreateDelegate(typeof(Func<IEnumerable<int?>, int?>)) },
                { typeof(long?), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<long?>) }).CreateDelegate(typeof(Func<IEnumerable<long?>, long?>)) },
                { typeof(float?), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<float?>) }).CreateDelegate(typeof(Func<IEnumerable<float?>, float?>)) },
                { typeof(double?), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<double?>) }).CreateDelegate(typeof(Func<IEnumerable<double?>, double?>)) },
                { typeof(decimal?), EnumerableType.GetMethod("Sum", new Type[]{ typeof(IEnumerable<decimal?>) }).CreateDelegate(typeof(Func<IEnumerable<decimal?>, decimal?>)) },
            };
        }
        private static class sum_helper<T>
        {
            private static Func<IEnumerable<T>, T> sum_1;

            static sum_helper()
            {
                var type = typeof(T);
                if (sum_helper.methods.Keys.Contains(type))
                {
                    sum_1 = (Func<IEnumerable<T>, T>)sum_helper.methods[type];
                }
                else throw new ArgumentException($"{typeof(T).GetTypeString()} is not supported for tlinq.sum");
            }

            public static T Sum(IEnumerable<T> values) => sum_1(values);
            public static T Sum<T2>(IEnumerable<T2> values, Func<T2, T> selector) => sum_1(values.Select(selector));
        }

        public static T sum<T>(IEnumerable<T> values) => sum_helper<T>.Sum(values);
        public static TOut sum<TIn, TOut>(IEnumerable<TIn> values, Method method)
            => sum_helper<TOut>.Sum(values, x => (TOut)method.GetValue(new object[1] { x }));
        public static TOut sum<TIn, TOut>(IEnumerable<TIn> values, Method method, ExecutionContext.ContextWrap context)
            => sum_helper<TOut>.Sum(values, x => (TOut)method.GetValue(context.pred, new object[1] { x }));
        private static class average_helper
        {
            public static Type EnumerableType = typeof(Enumerable);
            public static Dictionary<Type, (int, Delegate)> methods = new Dictionary<Type, (int, Delegate)>()
            {
                { typeof(int), (1, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<int>)}).CreateDelegate(typeof(Func<IEnumerable<int>, double>))) },
                { typeof(int?), (2, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<int?>)}).CreateDelegate(typeof(Func<IEnumerable<int?>, double?>))) },
                { typeof(long), (1, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<long>) }).CreateDelegate(typeof(Func<IEnumerable<long>, double>))) },
                { typeof(long?), (2, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<long?>)}).CreateDelegate(typeof(Func<IEnumerable<long?>, double?>))) },
                { typeof(float), (3, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<float>) }).CreateDelegate(typeof(Func<IEnumerable<float>, float>))) },
                { typeof(float?), (4, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<float?>)}).CreateDelegate(typeof(Func<IEnumerable<float?>, float?>))) },
                { typeof(double), (1, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<double>) }).CreateDelegate(typeof(Func<IEnumerable<double>, double>))) },
                { typeof(double?), (2, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<double?>)}).CreateDelegate(typeof(Func<IEnumerable<double?>, double?>))) },
                { typeof(decimal), (5, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<decimal>) }).CreateDelegate(typeof(Func<IEnumerable<decimal>, decimal>))) },
                { typeof(decimal?), (6, EnumerableType.GetMethod("Average", new Type[]{typeof(IEnumerable<decimal?>)}).CreateDelegate(typeof(Func<IEnumerable<decimal?>, decimal?>))) },
            };
        }
        private static class average_helper<T>
        {
            private static int mode;
            private static Func<IEnumerable<T>, double> average_1;
            private static Func<IEnumerable<T>, double?> average_2;
            private static Func<IEnumerable<T>, float> average_3;
            private static Func<IEnumerable<T>, float?> average_4;
            private static Func<IEnumerable<T>, decimal> average_5;
            private static Func<IEnumerable<T>, decimal?> average_6;

            static average_helper()
            {
                var type = typeof(T);
                if (average_helper.methods.Keys.Contains(type))
                {
                    var m = average_helper.methods[type];
                    switch (mode = m.Item1)
                    {
                        case 1:
                            average_1 = (Func<IEnumerable<T>, double>)m.Item2;
                            break;
                        case 2:
                            average_2 = (Func<IEnumerable<T>, double?>)m.Item2;
                            break;
                        case 3:
                            average_3 = (Func<IEnumerable<T>, float>)m.Item2;
                            break;
                        case 4:
                            average_4 = (Func<IEnumerable<T>, float?>)m.Item2;
                            break;
                        case 5:
                            average_5 = (Func<IEnumerable<T>, decimal>)m.Item2;
                            break;
                        case 6:
                            average_6 = (Func<IEnumerable<T>, decimal?>)m.Item2;
                            break;
                        default:
                            throw new ArgumentException("Wrong mode");
                    }
                }
                else throw new ArgumentException($"{typeof(T).GetTypeString()} is not supported for tlinq.average");
            }

            public static object Average(IEnumerable<T> values)
            {
                switch (mode)
                {
                    case 1: return average_1(values);
                    case 2: return average_2(values);
                    case 3: return average_3(values);
                    case 4: return average_4(values);
                    case 5: return average_5(values);
                    case 6: return average_6(values);
                }
                return default;
            }
            public static object Average<T2>(IEnumerable<T2> values, Func<T2, T> selector)
            {
                switch (mode)
                {
                    case 1: return average_1(values.Select(selector));
                    case 2: return average_2(values.Select(selector));
                    case 3: return average_3(values.Select(selector));
                    case 4: return average_4(values.Select(selector));
                    case 5: return average_5(values.Select(selector));
                    case 6: return average_6(values.Select(selector));
                }
                return default;
            }
        }

        public static object average<T>(IEnumerable<T> values) => average_helper<T>.Average(values);
        public static object average<TIn, TOut>(IEnumerable<TIn> values, Method method)
            => average_helper<TOut>.Average(values, x => (TOut)method.GetValue(new object[1] { x }));
        public static object average<TIn, TOut>(IEnumerable<TIn> values, Method method, ExecutionContext.ContextWrap context)
            => average_helper<TOut>.Average(values, x => (TOut)method.GetValue(context.pred, new object[1] { x }));

        public static IEnumerable<long> range(long end)
        {
            for (var i = 0; i < end; i += 1)
                yield return i;
        }
        public static IEnumerable<long> range(long start, long end)
        {
            for (var i = start; i < end; i += 1)
                yield return i;
        }
        public static IEnumerable<long> range(long start, long end, long step)
        {
            for (var i = start; i < end; i += step)
                yield return i;
        }

        public static T aggregate<T>(IEnumerable<T> objects, Method method)
        {
            return objects.Aggregate((x, y) => (T)method.GetValue(new object[2] { x, y }));
        }
        public static T aggregate<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Aggregate((x, y) => (T)method.GetValue(context.pred, new object[2] { x, y }));
        }

        public static T max<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Max(x => (T)method.GetValue(context.pred, new object[] { x }));
        }
        public static T max<T>(IEnumerable<T> objects, Func<T, T> func)
        {
            return objects.Max(x => func(x));
        }
        public static T max<T>(IEnumerable<T> objects) => objects.Max();

        public static T min<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Min(x => (T)method.GetValue(context.pred, new object[] { x }));
        }
        public static T min<T>(IEnumerable<T> objects, Method method)
        {
            return objects.Min(x => (T)method.GetValue(new object[] { x }));
        }
        public static T min<T>(IEnumerable<T> objects) => objects.Min();

        public static T max_by<T>(IEnumerable<T> objects, Method method)
            => objects.MaxBy(x => method.GetValue(new object[1] { x }));
        public static T max_by<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
            => objects.MaxBy(x => method.GetValue(context.pred, new object[1] { x }));

        public static T min_by<T>(IEnumerable<T> objects, Method method)
            => objects.MinBy(x => method.GetValue(new object[1] { x }));
        public static T min_by<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
            => objects.MinBy(x => method.GetValue(context.pred, new object[1] { x }));

        public static IEnumerable<TOut> select<TIn, TOut>(IEnumerable<TIn> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Select(x => (TOut)method.GetValue(context.pred, new object[] { x }));
        }
        public static IEnumerable<TOut> select<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> method)
        {
            return objects.Select(x => method.Invoke(x));
        }

        public static IEnumerable<T> select_many<T>(IEnumerable<T> objects) => objects.Select(x => (IEnumerable<T>)x).SelectMany(x => x);
        public static IEnumerable<TOut> select_many<TIn, TOut>(IEnumerable<TIn> objects, Method method)
        {
            return objects.Select(x => (IEnumerable<TIn>)x).SelectMany(x => method.GetValue(new object[1] { x }) as IEnumerable<TOut>);
        }
        public static IEnumerable<TOut> select_many<TIn, TOut>(IEnumerable<TIn> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Select(x => (IEnumerable<TIn>)x).SelectMany(x => method.GetValue(context.pred, new object[1] { x }) as IEnumerable<TOut>);
        }

        public static bool any<T>(IEnumerable<T> objects) => objects.Any();
        public static bool any<T>(IEnumerable<T> objects, Method method)
        {
            return objects.Any(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static bool any<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Any(x => (bool)method.GetValue(context.pred, new object[1] { x }));
        }

        public static bool all<T>(IEnumerable<T> objects, Method method)
        {
            return objects.All(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static bool all<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.All(x => (bool)method.GetValue(context.pred, new object[1] { x }));
        }

        public static IEnumerable<T> where<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Where(x => method.GetValue(context.pred, new object[] { x }).Cast<bool>());
        }
        public static IEnumerable<T> where<T>(IEnumerable<T> objects, Method method)
        {
            return objects.Where(x => method.GetValue(new object[] { x }).Cast<bool>());
        }

        public static long count<T>(IEnumerable<T> objects) => objects.Count();
        public static long count<T>(IEnumerable<T> objects, Method method)
        {
            return objects.Count(x => (bool)method.GetValue(new object[1] { x }));
        }
        public static long count<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context)
        {
            return objects.Count(x => (bool)method.GetValue(context.pred, new object[1] { x }));
        }

        public static IEnumerable<(int, T)> enumerate<T>(IEnumerable<T> objects)
        {
            var i = 0;
            foreach (var x in objects)
            {
                yield return (i, x);
                i++;
            }
        }

        public static Dictionary<TKey, TValue> to_dict<T, TKey, TValue>(IEnumerable<T> objects, Method methodKey, Method methodValue)
            => objects.ToDictionary(x => (TKey)methodKey.GetValue(new object[1] { x }), x => (TValue)methodValue.GetValue(new object[1] { x }));
        public static Dictionary<TKey, TValue> to_dict<T, TKey, TValue>(IEnumerable<T> objects, Method methodKey, Method methodValue, ExecutionContext.ContextWrap context)
            => objects.ToDictionary(x => (TKey)methodKey.GetValue(context.pred, new object[1] { x }), x => (TValue)methodValue.GetValue(context.pred, new object[1] { x }));
        public static List<T> to_list<T>(IEnumerable<T> objects) => objects.ToList();
        public static ITuple to_tuple<T>(IEnumerable<T> objects) => CreatorTuple.Create(objects.Select(x => x as object).ToArray());
        public static T[] to_array<T>(IEnumerable<T> objects) => objects.ToArray();
        public static IEnumerable<T> to_enumerable<T>(System.Collections.IEnumerable enumerable)
        {
            foreach (var x in enumerable)
                yield return (T)x;
        }
        public static IEnumerable<(TKey, TValue)> as_tuples<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            foreach (var x in dictionary.Keys)
                yield return (x, dictionary[x]);
        }

        public static IEnumerable<T> skip<T>(IEnumerable<T> objects, long count) => objects.Skip((int)count);
        public static IEnumerable<T> take<T>(IEnumerable<T> objects, long count) => objects.Take((int)count);

        public static T first<T>(IEnumerable<T> objects) => objects.First();
        public static T first<T>(IEnumerable<T> objects, Method method) => objects.First(x => (bool)method.GetValue(new object[1] { x }));
        public static T first<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context) => objects.First(x => (bool)method.GetValue(context.pred, new object[1] { x }));

        public static T last<T>(IEnumerable<T> objects) => objects.Last();
        public static T last<T>(IEnumerable<T> objects, Method method) => objects.Last(x => (bool)method.GetValue(new object[1] { x }));
        public static T last<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context) => objects.Last(x => (bool)method.GetValue(context.pred, new object[1] { x }));

        public static T first_or<T>(IEnumerable<T> objects, T or) where T : class => objects.FirstOrDefault() ?? or;
        public static T first_or<T>(IEnumerable<T> objects, Method method, T or) where T : class => objects.FirstOrDefault(x => (bool)method.GetValue(new object[1] { x })) ?? or;
        public static T first_or<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context, T or) where T : class => objects.FirstOrDefault(x => (bool)method.GetValue(context.pred, new object[1] { x })) ?? or;

        public static T last_or<T>(IEnumerable<T> objects, T or) where T: class => objects.LastOrDefault() ?? or;
        public static T last_or<T>(IEnumerable<T> objects, Method method, T or) where T : class => objects.LastOrDefault(x => (bool)method.GetValue(new object[1] { x })) ?? or;
        public static T last_or<T>(IEnumerable<T> objects, Method method, ExecutionContext.ContextWrap context, T or) where T : class => objects.LastOrDefault(x => (bool)method.GetValue(context.pred, new object[1] { x })) ?? or;

        public static T element_at<T>(IEnumerable<T> objects, long index) => objects.ElementAt((int)index);
        private static Random RandomFor_element_rand = new Random();
        public static T element_rand<T>(IList<T> objects)
        {
            return objects[RandomFor_element_rand.Next(objects.Count)];
        }

        public static string jts<T>(IEnumerable<T> objects, string str) => objects.JoinIntoString(str);
        public static string jts<T>(IEnumerable<T> objects) => objects.JoinIntoString(" ");

        public static T @foreach<T>(IEnumerable<T> objects, Method method)
        {
            var ret = default(T);
            foreach (var x in objects)
                ret = (T)method.GetValue(new object[1] { x });
            return ret;
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
