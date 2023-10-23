using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLThree;
using SLThree.Extensions;

namespace slt
{
    internal class Program
    {
        #region Arguments
        private static readonly Dictionary<string, string> ShortCommands = new Dictionary<string, string>()
        {
            { "-s", "--specification" },
            { "-v", "--version" },
            { "-d", "--difference" },
            { "-h", "--help" },
            { "-r", "--repl" },
        };

        private static string[] RunArguments;
        private static bool HasArgument(string arg) => RunArguments.Contains(arg.ReplaceAll(ShortCommands));
        private static bool TryGetArgument(string arg, out string value, Func<string> not_found = null)
        {
            arg = arg.ReplaceAll(ShortCommands);
            value = null;
            var ind = Array.FindIndex(RunArguments, x => x == arg);

            if (ind == -1) return false;
            if (RunArguments.Length > ind + 1 && !RunArguments[ind + 1].StartsWith("--"))
            {
                value = RunArguments[ind + 1];
                return true;
            }
            else
            {
                if (not_found == null) return false;
                else
                {
                    value = not_found();
                    return true;
                }
            }
        }

        public static void OutCurrentVersion()
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(SLTVersion.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {SLTVersion.VersionWithoutRevision} ");
            Console.ForegroundColor = CurrentEditionColor;
            Console.WriteLine($"{SLTVersion.Edition}");
            Console.ForegroundColor = old;
            var time = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(SLTVersion.LastUpdate), TimeZoneInfo.Local).ToString("dd.MM.yy HH:mm");
            Console.Write("rev ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{SLTVersion.Revision}");
            Console.ForegroundColor = old;
            Console.Write($" by ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{time}");
            Console.ForegroundColor = old;
        }

        public static void OutVersion(string version)
        {
            if (DocsIntergration.VersionsData.TryGetValue(version.Replace("last", SLTVersion.VersionWithoutRevision), out var data))
            {
                Console.WriteLine(data.JoinIntoString("\n"));
            }
            else Console.WriteLine($"Version {version} not found");
        }

        public static void OutDifference(int count)
        {
            var lv = count >= 0 ? DocsIntergration.VersionsData.Count - count : 0;
            if (lv < 0) lv = 0;
            var i = DocsIntergration.VersionsData.Count - 1;
            foreach (var entry in DocsIntergration.VersionsData.Reverse())
            {
                if (i < lv) break;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                i--;
            }
        }

        public static void OutDifferenceBy(string version)
        {
            foreach (var entry in DocsIntergration.VersionsData.Reverse())
            {
                if (entry.Key.StartsWith(version)) return;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
            }
        }

        public static void OutSpecification()
        {
            Console.WriteLine(DocsIntergration.Specification.JoinIntoString("\n"));
        }

        public static void OutHelp()
        {
            Console.WriteLine(DocsIntergration.Help.JoinIntoString("\n"));
        }
        #endregion
        public static void StartREPL()
        {
            var old = Console.ForegroundColor;
            Console.Title = $"{SLTVersion.Name} REPL";
            OutCurrentVersion();
            Console.WriteLine($"Maded by Alexandr Kotov. Pegasus is cool!");

            var parser = new SLThree.Parser();
            var executionContext = new ExecutionContext();
            while (true)
            {
                Console.Write(">>> ");
                var code = Console.ReadLine();
                if (code == "quit()") return;
                try
                {
                    var st = parser.Parse(code);
                    Console.WriteLine(st.GetValue(executionContext) ?? "null");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
                    Console.WriteLine(e.ToString());
#else
                    Console.WriteLine(e.GetType().FullName + ": " + e.Message);
#endif
                    Console.ForegroundColor = old;
                }
            }
        }

        public static ConsoleColor CurrentEditionColor = ConsoleColor.Yellow;

        public static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            args = RunArguments = Array.ConvertAll(args, x => x.StartsWith("-") && !x.StartsWith("--") ? x.ReplaceAll(ShortCommands) : x);

            if (HasArgument("-r") || args.Length == 0) StartREPL();
            if (TryGetArgument("-v", out var version)) OutVersion(version);
            else if (HasArgument("-v")) OutCurrentVersion();
            if (TryGetArgument("-d", out var last_versions, () => int.MaxValue.ToString()))
            {
                if (int.TryParse(last_versions, out var lasts)) OutDifference(lasts);
                else OutDifferenceBy(last_versions);
            }
            if (HasArgument("-s")) OutSpecification();
            if (HasArgument("-h")) OutHelp();
        }
    }
}
