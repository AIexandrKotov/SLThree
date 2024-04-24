using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    //Typed linq with delegates
    public class dlinq
    {
        public static T sum<T>(IEnumerable<T> objects)
            => tlinq.sum_helper<T>.Sum(objects);
        public static TOut sum<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> selector)
            => tlinq.sum_helper<TOut>.Sum(objects, selector);
        public static object average<T>(IEnumerable<T> objects)
            => tlinq.average_helper<T>.Average(objects);
        public static object average<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> selector)
            => tlinq.average_helper<TOut>.Average(objects, selector);

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

        public static T aggregate<T>(IEnumerable<T> objects, Func<T, T, T> aggregator)
        {
            return objects.Aggregate(aggregator);
        }

        public static TOut max<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> func)
        {
            return objects.Max(func);
        }
        public static T max<T>(IEnumerable<T> objects) => objects.Max();

        public static TOut min<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> func)
        {
            return objects.Min(func);
        }
        public static T min<T>(IEnumerable<T> objects) => objects.Min();

        public static TIn max_by<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> func)
            => objects.MaxBy(func);
        public static TIn min_by<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> func)
            => objects.MinBy(func);

        public static IEnumerable<TOut> select<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, TOut> selector)
        {
            return objects.Select(selector);
        }

        public static IEnumerable<T> select_many<T>(IEnumerable<T> objects) => objects.Select(x => (IEnumerable<T>)x).SelectMany(x => x);
        public static IEnumerable<TOut> select_many<TIn, TOut>(IEnumerable<TIn> objects, Func<TIn, IEnumerable<TOut>> func) => objects.SelectMany(func);

        public static bool any<T>(IEnumerable<T> objects) => objects.Any();
        public static bool any<T>(IEnumerable<T> objects, Func<T, bool> predicate)
        {
            return objects.Any(predicate);
        }

        public static bool all<T>(IEnumerable<T> objects, Func<T, bool> predicate)
        {
            return objects.All(predicate);
        }

        public static IEnumerable<T> where<T>(IEnumerable<T> objects, Func<T, bool> predicate)
        {
            return objects.Where(predicate);
        }

        public static long count<T>(IEnumerable<T> objects) => objects.Count();
        public static long count<T>(IEnumerable<T> objects, Func<T, bool> selector)
        {
            return objects.Count(selector);
        }

        public static IEnumerable<(long, T)> enumerate<T>(IEnumerable<T> objects)
        {
            var i = 0L;
            foreach (var x in objects)
            {
                yield return (i, x);
                i++;
            }
        }

        public static Dictionary<TKey, TValue> to_dict<T, TKey, TValue>(IEnumerable<T> objects, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
            => objects.ToDictionary(keySelector, valueSelector);
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
        public static T first<T>(IEnumerable<T> objects, Func<T, bool> predicate) => objects.First(predicate);

        public static T last<T>(IEnumerable<T> objects) => objects.Last();
        public static T last<T>(IEnumerable<T> objects, Func<T, bool> predicate) => objects.Last(predicate);

        public static T first_or<T>(IEnumerable<T> objects, T or) where T : class => objects.FirstOrDefault() ?? or;
        public static T first_or<T>(IEnumerable<T> objects, Func<T, bool> predicate, T or) where T : class => objects.FirstOrDefault(predicate) ?? or;

        public static T last_or<T>(IEnumerable<T> objects, T or) where T : class => objects.LastOrDefault() ?? or;
        public static T last_or<T>(IEnumerable<T> objects, Func<T, bool> predicate, T or) where T : class => objects.LastOrDefault(predicate) ?? or;

        public static T element_at<T>(IEnumerable<T> objects, long index) => objects.ElementAt((int)index);
        private static Random RandomFor_element_rand = new Random();
        public static T element_rand<T>(IList<T> objects)
        {
            return objects[RandomFor_element_rand.Next(objects.Count)];
        }

        public static IOrderedEnumerable<T> order_by<T, TKey>(IEnumerable<T> objects, Func<T, TKey> keySelector)
            => objects.OrderBy(keySelector);
        public static IOrderedEnumerable<T> order_by_desc<T, TKey>(IEnumerable<T> objects, Func<T, TKey> keySelector)
            => objects.OrderByDescending(keySelector);
        public static IEnumerable<IGrouping<TKey, T>> group_by<T, TKey>(IEnumerable<T> objects, Func<T, TKey> keySelector)
            => objects.GroupBy(keySelector);

        public static string jts<T>(IEnumerable<T> objects, string str) => objects.JoinIntoString(str);
        public static string jts<T>(IEnumerable<T> objects) => objects.JoinIntoString(" ");

        public static void @foreach<T>(IEnumerable<T> objects, Action<T> action)
        {
            foreach (var x in objects)
                action(x);
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
