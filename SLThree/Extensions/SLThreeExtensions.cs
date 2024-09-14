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

        public static object EvalExpression(this LanguageInformation.IParser parser, string s, ExecutionContext context = null)
        {
            if (context == null) context = new ExecutionContext();
            return parser.ParseExpression(s, null).GetValue(context);
        }

        private static ExecutionContext InitPreseted(ExecutionContext preset)
        {
            var ret = new ExecutionContext(false, false);
            if (preset != null) ret.implement(preset);
            return ret;
        }

        public static ExecutionContext RunScript(this LanguageInformation.IParser parser, string s, string filename = null, ExecutionContext context = null, ExecutionContext preset = null)
        {
            var parsed = parser.ParseScript(s, filename);
            var ret = context ?? InitPreseted(preset);
            parsed.GetValue(ret);
            return ret;
        }
    }
}
