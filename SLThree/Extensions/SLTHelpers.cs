using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Extensions
{
    internal static class SLTHelpers
    {
        public static object CastToMax(this object o)
        {
            switch (o)
            {
                case bool v: return v ? 1 : (long)0;
                case float v: return (double)v;
                case sbyte v: return (long)v;
                case short v: return (long)v;
                case int v: return (long)v;
                case byte v: return (ulong)v;
                case ushort v: return (ulong)v;
                case uint v: return (ulong)v;
            }
            return o;
        }

        public static long ToLong(this bool b) => b ? 1 : 0;
    }
}
