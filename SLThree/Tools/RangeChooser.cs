using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections
{
    public class RangeChooser : IChooser<long>, IChooser<object>
    {
        public static RNGCryptoServiceProvider RandomNumberGenerator = new RNGCryptoServiceProvider();
        public long LowerBound { get; set; } = 0;
        public long UpperBound { get; set; } = 2;

        private byte[] buffer = new byte[8];

        public RangeChooser(long lower, long upper)
        {
            LowerBound = lower;
            UpperBound = upper;
        }

        public long Choose()
        {
            RandomNumberGenerator.GetBytes(buffer);
            return Math.Abs(BitConverter.ToInt64(buffer, 0) % (UpperBound - LowerBound + 1)) + LowerBound;
        }

        object IChooser.Choose()
        {
            return Choose();
        }

        public IEnumerator<long> GetEnumerator()
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