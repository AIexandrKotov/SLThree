using System;
using System.IO;

namespace SLThree.Extensions
{
    public static class SLThreeExtensions
    {
        public static string ToDynamicPercents(this double d)
        {
            var precision = 0;

            while (d * Math.Pow(10, precision) != Math.Round(d * Math.Pow(10, precision)))
                precision++;

            if (precision < 2) precision = 2;

            return d.ToString($"P{precision - 2}");
        }

        public static string ReadString(this Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
        public static string[] ReadStrings(this Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
            }
        }
    }
}
