using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SLThree.Extensions;
using System.Threading;

namespace SLThree
{
    public class Locale
    {
        public static Locale Current 
        { 
            get => current; 
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                current = value;
            }
        }

        public static readonly Dictionary<string, Locale> RegistredLocales;
        public static readonly Locale Default;
        private static Locale current;

        public readonly string Identifier;
        public readonly string DefaultName;
        public readonly string Name;
        public readonly ConcurrentDictionary<string, string> Strings;

        public string this[string str]
        {
            get => Strings.TryGetValue(str, out var value) ? value : (this != Default) ? Default[str] : str;
        }

        public Locale(string id, ConcurrentDictionary<string, string> strings)
        {
            Identifier = id;
            Strings = strings;
            DefaultName = strings["Locale_DefaultName"];
            Name = strings["Locale_Name"];
        }

        public static KeyValuePair<string, string> SplitByFirst(string str, char c)
        {
            var ind = str.IndexOf(c);
            if (c == -1) throw new ArgumentException($"'{c}' not found");
            return new KeyValuePair<string, string>(str.Substring(0, ind), str.Substring(ind + 1, str.Length - ind - 1));
        }

        static Locale()
        {
            var ass = Assembly.GetExecutingAssembly();
            RegistredLocales = ass
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("SLThree._locales."))
                .Select(
                    x => {
                        using (var stream = ass.GetManifestResourceStream(x))
                        {
                            var script = stream.ReadStrings().Where(str => !string.IsNullOrWhiteSpace(str)).Select(str => SplitByFirst(str, '='));
                            return new Locale(
                                Path.GetFileNameWithoutExtension(x).Replace("SLThree._locales.", ""),
                                new ConcurrentDictionary<string, string>(script)
                            );
                        }
                    }
                )
                .ToDictionary(x => x.Identifier);
            Default = RegistredLocales.TryGetValue("en", out var en) ? en : RegistredLocales.First().Value;
            SetLocaleBasedOnCulture();
        }

        public static void SetLocaleBasedOnCulture(string culture = null)
        {
            culture = culture ?? Thread.CurrentThread.CurrentCulture.Name;
            Current = RegistredLocales.FirstOrDefault(x => x.Value["Cultures"].Split(';').Contains(culture)).Value ?? Default;
        }

        public override string ToString() => $"[{Identifier}] {Name} ({DefaultName}), {Strings.Count} lines";
    }
}
