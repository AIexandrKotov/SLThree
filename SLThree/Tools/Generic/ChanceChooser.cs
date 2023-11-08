using SLThree.Tools.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.Tools.Generic
{
    public class ChanceChooser<T> : IChooser<T>
    {
        public static Random Random = new Random();

        public IList<(T, double)> Values { get; set; }

        public ChanceChooser(IList<(T, double)> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Count == 0) throw new ArgumentException($"Count of elements must be > 0");
            var sum = values.Sum(x => x.Item2);
            Values = values.Select(x => (x.Item1, x.Item2 / sum)).ToArray();
        }

        public ChanceChooser(params (T, double)[] values) : this(values as IList<(T, double)>) { }

        public T Choose()
        {
            var t = Random.NextDouble();
            var sum = 0.0;
            for (var i = 0; i < Values.Count; i++)
            {
                sum += Values[i].Item2;
                if (sum >= t) return Values[i].Item1;
            }
            return Values[Values.Count - 1].Item1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            while (true) yield return Choose();
        }

        object IChooser.Choose()
        {
            var t = Random.NextDouble();
            var sum = 0.0;
            for (var i = 0; i < Values.Count; i++)
            {
                sum += Values[i].Item2;
                if (sum >= t) return Values[i].Item1;
            }
            return Values[Values.Count - 1].Item1;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            while (true) yield return Choose();
        }
    }
}