using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            { "-e", "--encoding" },
            { "-V", "--repl-version" },
            { "-D", "--repl-difference" },
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
        private static string GetArgument(string arg) => TryGetArgument(arg, out var value) ? value : null;

        public static Encoding GetEncoding(string str)
        {
            if (str == null) return Encoding.UTF8;
            else if (PublicEncodings.TryGetValue(str, out var encoding)) return encoding;
            else
            {
                try
                {
                    if (int.TryParse(str, out var result)) return Encoding.GetEncoding(result);
                    else return Encoding.GetEncoding(str);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"Encoding {str} not found. using utf-8");
                    return Encoding.UTF8;
                }
            }
        }
        private static Dictionary<string, Encoding> PublicEncodings = new Dictionary<string, Encoding>()
        {
            { "utf-8", Encoding.UTF8 },
            { "utf-16", Encoding.Unicode },
            { "unicode", Encoding.Unicode },
            { "ansi", Encoding.GetEncoding(1250) },
        };

        public static void OutCurrentVersion()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(REPLVersion.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {REPLVersion.VersionWithoutRevision} ");
            Console.ResetColor();
            var time2 = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(REPLVersion.LastUpdate), TimeZoneInfo.Local).ToString("dd.MM.yy HH:mm");
            Console.Write("rev ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{REPLVersion.Revision}");
            Console.ResetColor();
            Console.Write($" by ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{time2}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(SLTVersion.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {SLTVersionWithoutRevision} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{SLTVersion.Edition} ");
            Console.ResetColor();
            var time = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(SLTTime), TimeZoneInfo.Local).ToString("dd.MM.yy HH:mm");
            Console.Write("rev ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{SLTRevision}");
            Console.ResetColor();
            Console.Write($" by ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{time}");
            Console.ResetColor();

        }

        public static string SLTVersionWithoutRevision;
        public static string SLTRevision;
        public static long SLTTime;
        static Program()
        {
            var ass = Assembly.GetAssembly(typeof(SLTVersion));
            var ver = ass.GetName().Version;
            SLTVersionWithoutRevision = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            SLTRevision = ver.Revision.ToString();
            var sltver = ass.GetType("SLTVersion");
            SLTTime = sltver.GetField("LastUpdate").GetValue(null).Cast<long>();
            SLThreeVersions = sltver.GetProperty("VersionsData").GetValue(null).Cast<SortedDictionary<string, string[]>>();
            Specification = sltver.GetProperty("Specification").GetValue(null).Cast<string[]>();
        }

        private static SortedDictionary<string, string[]> SLThreeVersions;
        private static string[] Specification;

        public static void OutVersion(string version)
        {
            if (SLThreeVersions.TryGetValue(version.Replace("last", SLTVersionWithoutRevision), out var data))
            {
                Console.WriteLine(data.JoinIntoString("\n"));
            }
            else Console.WriteLine($"Version {version} not found");
        }

        public static void OutREPLVersion(string version)
        {
            if (DocsIntergration.REPLVersionsData.TryGetValue(version.Replace("last", SLTVersionWithoutRevision), out var data))
            {
                Console.WriteLine(data.JoinIntoString("\n"));
            }
            else Console.WriteLine($"Version {version} not found");
        }

        public static void OutDifference(int count)
        {
            var lv = count >= 0 ? SLThreeVersions.Count - count : 0;
            if (lv < 0) lv = 0;
            var i = SLThreeVersions.Count - 1;
            foreach (var entry in SLThreeVersions.Reverse())
            {
                if (i < lv) break;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                i--;
            }
        }

        public static void OutDifferenceBy(string version)
        {
            foreach (var entry in SLThreeVersions.Reverse())
            {
                if (entry.Key.StartsWith(version)) return;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
            }
        }

        public static void OutREPLDifference(int count)
        {
            var lv = count >= 0 ? DocsIntergration.REPLVersionsData.Count - count : 0;
            if (lv < 0) lv = 0;
            var i = DocsIntergration.REPLVersionsData.Count - 1;
            foreach (var entry in DocsIntergration.REPLVersionsData.Reverse())
            {
                if (i < lv) break;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                i--;
            }
        }

        public static void OutREPLDifferenceBy(string version)
        {
            foreach (var entry in DocsIntergration.REPLVersionsData.Reverse())
            {
                if (entry.Key.StartsWith(version)) return;
                Console.WriteLine($"==> {entry.Key}");
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
            }
        }

        public static void OutSpecification()
        {
            Console.WriteLine(Specification.JoinIntoString("\n"));
        }

        public static void OutHelp()
        {
            Console.WriteLine(DocsIntergration.Help.JoinIntoString("\n"));
        }
        #endregion
        
        public static void OutException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
            Console.WriteLine(e.ToString());
#else
            Console.WriteLine(e.GetType().FullName + ": " + e.Message);
#endif
            Console.ResetColor();
        }

        public static ExecutionContext InvokeFile(string filename, Encoding encoding = null, bool show_result = true)
        {
            var parser = new SLThree.Parser();
            var executionContext = new ExecutionContext();
            try
            {
                var st = parser.ParseScript(File.ReadAllText(filename, encoding ?? Encoding.UTF8), filename);
                var o = st.GetValue(executionContext) ?? "null";
                if (o is string) o = $"\"{o}\"";
                if (show_result) Console.WriteLine(o);
            }
            catch (UnauthorizedAccessException) when (Directory.Exists(filename)) { Console.WriteLine($"\"{filename}\" is directory. For now REPL does not support directories!"); }
            catch (FileNotFoundException) { Console.WriteLine($"File \"{filename}\" not found."); }
            catch (Exception e)
            {
                OutException(e);
            }
            return executionContext;
        }

        public static void OutREPLInfo()
        {
            Console.Title = $"{REPLVersion.Name}";
            OutCurrentVersion();
            Console.WriteLine($"Maded by Alexandr Kotov. Pegasus is cool!");
        }

        public static void StartREPL(ExecutionContext myExecutionContext = null)
        {
            OutREPLInfo();

            var parser = new SLThree.Parser();
            var executionContext = myExecutionContext ?? new ExecutionContext();
            while (true)
            {
                Console.Write(">>> ");
                var code = Console.ReadLine();
                if (code == "quit();") return;
                if (code == "clear();")
                {
                    Console.Clear();
                    OutREPLInfo();
                    continue;
                }
                if (code == "reset();")
                {
                    executionContext = new ExecutionContext();
                    continue;
                }
                try
                {
                    var st = parser.ParseScript(code);
                    Console.WriteLine(st.GetValue(executionContext) ?? "null");
                }
                catch (Exception e)
                {
                    OutException(e);
                }
            }
        }

        public static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            args = RunArguments = Array.ConvertAll(args, x => x.StartsWith("-") && !x.StartsWith("--") ? x.ReplaceAll(ShortCommands) : x);

            if (args.Length > 0 && !args[0].StartsWith("-"))
            {
                var encoding = GetEncoding(GetArgument("-e"));
                if (HasArgument("-r"))
                {
                    var context = InvokeFile(args[0], encoding, false);
                    StartREPL(context);
                }
                else InvokeFile(args[0], encoding, true);
            }
            if (HasArgument("-r") || args.Length == 0) StartREPL();
            if (TryGetArgument("-v", out var version)) OutVersion(version);
            else if (HasArgument("-v")) OutCurrentVersion();
            if (TryGetArgument("-V", out var repl_version)) OutREPLVersion(repl_version);
            if (TryGetArgument("-d", out var last_versions, () => int.MaxValue.ToString()))
            {
                if (int.TryParse(last_versions, out var lasts)) OutDifference(lasts);
                else OutDifferenceBy(last_versions);
            }
            if (TryGetArgument("-D", out var last_repl_versions, () => int.MaxValue.ToString()))
            {
                if (int.TryParse(last_repl_versions, out var lasts)) OutREPLDifference(lasts);
                else OutREPLDifferenceBy(last_repl_versions);
            }
            if (HasArgument("-s")) OutSpecification();
            if (HasArgument("-h")) OutHelp();
        }
    }
}
