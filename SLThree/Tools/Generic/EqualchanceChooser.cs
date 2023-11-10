using System.Linq;

namespace System.Collections.Generic
{
    public class EqualchanceChooser<T> : IEqualchanceChooser<T>
    {
        public static Random Random = new Random();

        public IList<T> Values { get; set; }

        public EqualchanceChooser(IList<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Count == 0) throw new ArgumentException($"Count of elements must be > 0");
            Values = values;
        }

        public EqualchanceChooser(params T[] values) : this(values as IList<T>) { }

        object IChooser.Choose() => Values[Random.Next(Values.Count)];
        public T Choose() => Values[Random.Next(Values.Count)];

        public IEnumerator<T> GetEnumerator()
        {
            while (true) yield return Choose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            while (true) yield return Choose();
        }

        IList<object> IEqualchanceChooser.Values => Values.Select(x => (object)x).ToArray();
    }
}