using System.Collections.Generic;

namespace System.Collections
{
    public class BooleanChooser : IChooser<bool>, IChooser<object>
    {
        public static Random Random = new Random();

        public double Limit;

        public BooleanChooser(double limit)
        {
            Limit = limit;
        }

        public bool Choose()
        {
            return Random.NextDouble() < Limit;
        }

        object IChooser.Choose()
        {
            return Choose();
        }

        public IEnumerator<bool> GetEnumerator()
        {
            while (true) yield return Choose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            while (true) yield return Choose();
        }

        object IChooser<object>.Choose()
        {
            return Choose();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            while (true) yield return Choose();
        }
    }
}