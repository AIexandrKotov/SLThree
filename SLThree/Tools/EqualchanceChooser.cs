using System;
using System.Collections;
using System.Collections.Generic;

namespace SLThree.Tools
{
    public class EqualchanceChooser : IChooser
    {
        public static Random Random = new Random();

        public IList<object> Values { get; set; }

        public EqualchanceChooser(IList<object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Count == 0) throw new ArgumentException($"Count of elements must be > 0");
            Values = values;
        }

        public EqualchanceChooser(params object[] values) : this(values as IList<object>) { }

        public object Choose() => Values[Random.Next(Values.Count)];

        public IEnumerator GetEnumerator()
        {
            while (true) yield return Choose();
        }
    }

}