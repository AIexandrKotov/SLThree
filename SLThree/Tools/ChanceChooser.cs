using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.Tools
{
    public class ChanceChooser : IChooser
    {
        public static Random Random = new Random();

        public IList<(object, double)> Values { get; set; }

        public ChanceChooser(IList<(object, double)> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Count == 0) throw new ArgumentException($"Count of elements must be > 0");
            var sum = values.Sum(x => x.Item2);
            Values = values.Select(x => (x.Item1, x.Item2 / sum)).ToArray();
        }

        public ChanceChooser(params (object, double)[] values) : this(values as IList<(object, double)>) { }

        public object Choose()
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

        public IEnumerator GetEnumerator()
        {
            while (true) yield return Choose();
        }
    }

}