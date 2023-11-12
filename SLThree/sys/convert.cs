using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class convert
    {
        public static string bin(long num) => $"0b{Convert.ToString(num, 2)}";
        public static string oct(long num) => $"0o{Convert.ToString(num, 8)}";
        public static string hex(long num) => $"0x{Convert.ToString(num, 16)}";
        public static long fbin(string str) => Convert.ToInt64(str.Trim().Replace("0b", ""), 2);
        public static long foct(string str) => Convert.ToInt64(str.Trim().Replace("0o", ""), 8);
        public static long fhex(string str) => Convert.ToInt64(str.Trim().Replace("0x", ""), 16);
        public static long from(string str)
        {
            var trimmed = str.Trim();
            if (trimmed.StartsWith("0x")) return Convert.ToInt64(trimmed.Replace("0x", ""), 16);
            if (trimmed.StartsWith("0o")) return Convert.ToInt64(trimmed.Replace("0o", ""), 8);
            if (trimmed.StartsWith("0b")) return Convert.ToInt64(trimmed.Replace("0b", ""), 2);
            throw new ArgumentException($"\"{str}\" is not valid to convert");
        }
    }
#pragma warning restore IDE1006 // Стили именования
}