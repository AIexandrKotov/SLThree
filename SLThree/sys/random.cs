using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public class random
    {
        public static object choose(IChooser chooser) => chooser.Choose();
        public static object to_chooser(object o)
        {
            if (o is IDictionary dictionary)
            {
                var lst = new List<(object, double)>();
                foreach (var key in dictionary.Keys)
                    lst.Add((key, dictionary[key].Cast<double>()));
                return new ChanceChooser<object>(lst);
            }
            if (o is CreatorRange.RangeEnumerator enumerator) return new RangeChooser(enumerator.Lower, enumerator.Upper);
            if (o is double d) return new BooleanChooser(d);
            if (o is IEnumerable enumerable) return new EqualchanceChooser<object>(enumerable.Enumerate().ToArray());
            throw new ArgumentException($"{o?.GetType().GetTypeString() ?? "null"} is not convertable to chooser");
        }
        public static object to_chooser(object o, Type type)
        {
            if (o is IDictionary dictionary) return internal_chooser1reflected(dictionary, type);
            if (o is IEnumerable enumerable) return internal_chooser2reflected(enumerable, type);
            throw new ArgumentException($"{o?.GetType().GetTypeString() ?? "null"} is not convertable to chooser<{type.GetTypeString()}>");
        }

#pragma warning disable IDE0051 // Удалите неиспользуемые закрытые члены
        private static EqualchanceChooser<T> internal_chooser2<T>(IEnumerable enumerable)
        {
            var ret = new List<T>();
            foreach (var x in enumerable)
                ret.Add(x.CastToType<T>());
            return new EqualchanceChooser<T>(ret);
        }
        private static ChanceChooser<T> internal_chooser1<T>(IDictionary dictionary)
#pragma warning restore IDE0051 // Удалите неиспользуемые закрытые члены
        {
            var lst = new List<(T, double)>();
            foreach (var key in dictionary.Keys)
                lst.Add((key.CastToType<T>(), dictionary[key].Cast<double>()));
            return new ChanceChooser<T>(lst);
        }
        private static readonly MethodInfo i_c1 = typeof(random).GetMethod("internal_chooser1", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo i_c2 = typeof(random).GetMethod("internal_chooser2", BindingFlags.NonPublic | BindingFlags.Static);
        private static object internal_chooser1reflected(IDictionary dictionary, Type type)
            => i_c1.MakeGenericMethod(new Type[1] { type }).Invoke(null, new object[1] { dictionary });
        private static object internal_chooser2reflected(IEnumerable enumerable, Type type)
            => i_c2.MakeGenericMethod(new Type[1] { type }).Invoke(null, new object[1] { enumerable });
    }
#pragma warning restore IDE1006 // Стили именования
}
