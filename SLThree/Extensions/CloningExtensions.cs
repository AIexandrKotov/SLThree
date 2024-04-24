using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SLThree.Extensions.Cloning
{
    public static class CloningExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CloneCast<T>(this T o) where T : ICloneable => o == null ? default : (T)o.Clone();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CloneCast<T>(this ICloneable o) => o == null ? default : (T)o.Clone();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CloneCastUnsafe<T>(this T o) where T : ICloneable => (T)o.Clone();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CloneCastUnsafe<T>(this ICloneable o) => (T)o.Clone();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Copy<T>(this T o) where T : struct => o;

        /// <returns>Скопированная последовательность</returns>
        public static IEnumerable<T> Copy<T>(this IEnumerable<T> enumerable)
            where T : struct
        {
            foreach (var item in enumerable)
                yield return item.Copy();
        }
        /// <returns>Склонированная последовательность</returns>
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> enumerable)
            where T : ICloneable
        {
            foreach (var item in enumerable)
                yield return item.CloneCast();
        }

        /// <returns>Скопированный массив</returns>
        public static List<T> Copy<T>(this List<T> list)
            where T : struct
        {
            return list.ConvertAll(x => x.Copy());
        }
        /// <returns>Скопированный массив</returns>
        public static List<T> ReferenceCopy<T>(this List<T> list)
            where T : class
        {
            return new List<T>(list);
        }
        /// <returns>Склонированный массив</returns>
        public static List<T> Clone<T>(this List<T> list)
            where T : ICloneable
        {
            return list.ConvertAll(x => x.CloneCast());
        }

        /// <returns>Скопированный массив</returns>
        public static T[] Copy<T>(this T[] array)
            where T : struct
        {
            var ret = new T[array.Length];
            array.CopyTo(ret, 0);
            return ret;
        }
        /// <returns>Скопированный массив</returns>
        public static T[] ReferenceCopy<T>(this T[] array)
            where T : class
        {
            var ret = new T[array.Length];
            array.CopyTo(ret, 0);
            return ret;
        }
        /// <returns>Склонированный массив</returns>
        public static T[] CloneArray<T>(this T[] array)
            where T : ICloneable
        {
            return array.ConvertAll(x => x.CloneCast());
        }

        /// <returns>Скопированный словарь</returns>
        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : struct
            where TValue : struct
        {
            return new Dictionary<TKey, TValue>(dict);
        }
        /// <returns>Скопированный словарь</returns>
        public static Dictionary<TKey, TValue> ReferenceCopy<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : class
            where TValue : class
        {
            return new Dictionary<TKey, TValue>(dict);
        }
        /// <returns>Скопированный словарь</returns>
        public static Dictionary<TKey, TValue> ReferenceCopyByKey<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : class
            where TValue : struct
        {
            return new Dictionary<TKey, TValue>(dict);
        }
        /// <returns>Скопированный словарь</returns>
        public static Dictionary<TKey, TValue> ReferenceCopyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : struct
            where TValue : class
        {
            return new Dictionary<TKey, TValue>(dict);
        }

        /// <returns>Склонированный словарь</returns>
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : ICloneable
            where TValue : ICloneable
        {
            return dict.ToDictionary(x => x.Key.CloneCast(), x => x.Value.CloneCast());
        }
        /// <returns>Склонированный словарь</returns>
        public static Dictionary<TKey, TValue> CloneByKey<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : ICloneable
            where TValue : struct
        {
            return dict.ToDictionary(x => x.Key.CloneCast(), x => x.Value.Copy());
        }
        /// <returns>Склонированный словарь</returns>
        public static Dictionary<TKey, TValue> CloneByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : struct
            where TValue : ICloneable
        {
            return dict.ToDictionary(x => x.Key.Copy(), x => x.Value.CloneCast());
        }
        /// <returns>Склонированный словарь</returns>
        public static Dictionary<TKey, TValue> ReferenceCloneByKey<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : ICloneable
            where TValue : class
        {
            return dict.ToDictionary(x => x.Key.CloneCast(), x => x.Value);
        }
        /// <returns>Склонированный словарь</returns>
        public static Dictionary<TKey, TValue> ReferenceCloneByValue<TKey, TValue>(this Dictionary<TKey, TValue> dict)
            where TKey : class
            where TValue : ICloneable
        {
            return dict.ToDictionary(x => x.Key, x => x.Value.CloneCast());
        }
    }
}
