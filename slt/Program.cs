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
        static Program()
        {
            InitSLThreeAssemblyInfo();
        }

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
        private static Dictionary<string, Encoding> EncodingAliases = new Dictionary<string, Encoding>()
        {
            { "utf-8", Encoding.UTF8 },
            { "utf-16", Encoding.Unicode },
            { "unicode", Encoding.Unicode },
            { "ansi", Encoding.GetEncoding(1250) },
        };
        private static string SLTVersionWithoutRevision;
        private static string SLTRevision;
        private static long SLTTime;
        private static SortedDictionary<string, string[]> SLThreeVersions;
        private static string[] Specification;
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
            else if (EncodingAliases.TryGetValue(str, out var encoding)) return encoding;
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
        private static void InitSLThreeAssemblyInfo()
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

        #region Outs
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

        public static void OutREPLHelp()
        {
            Console.WriteLine(DocsIntergration.REPLHelp.JoinIntoString("\n"));
        }

        public static void OutHelp()
        {
            Console.WriteLine(DocsIntergration.Help.JoinIntoString("\n"));
        }
        #endregion

        #endregion

        #region Universal outs
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

        public static void OutAsException(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public static void OutAsOutput(object value)
        {
            if (value is string) value = $"\"{value}\"";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(value ?? "null");
            Console.ResetColor();
        }
        #endregion

        #region Compiler and Interpreter
        public static ExecutionContext InvokeFile(string filename, Encoding encoding = null, bool show_result = true)
        {
            var parser = new SLThree.Parser();
            var executionContext = new ExecutionContext();
            try
            {
                var st = parser.ParseScript(File.ReadAllText(filename, encoding ?? Encoding.UTF8), filename);
                if (show_result) OutAsOutput(st.GetValue(executionContext));
            }
            catch (UnauthorizedAccessException) when (Directory.Exists(filename)) { OutAsException($"\"{filename}\" is directory. For now REPL does not support directories!"); }
            catch (FileNotFoundException) { OutAsException($"File \"{filename}\" not found."); }
            catch (Exception e)
            {
                OutException(e);
            }
            return executionContext;
        }
        #endregion

        #region REPL

        #region REPL Commands

        public static Dictionary<string, string> ShortREPLCommands = new Dictionary<string, string>()
        {
            { "h", "help" },
            { "q", "quit" },
            { "c", "clear" },
            { "r", "reset" },
        };
        public static Dictionary<string, Action<string>> REPLCommands = new Dictionary<string, Action<string>>()
        {
            { "help", s => OutREPLHelp() },
            { "quit", s => REPLLoop = false },
            { "clear", s => { Console.Clear(); OutREPLInfo(); } },
            { "reset", s => REPLContext = new ExecutionContext() },
        };
        public static string REPLApplyShort(string command)
        {
            foreach (var x in ShortCommands)
                if (command == x.Key) return x.Value;
            return command;
        }
        public static void REPLCommand(string command)
        {
            command = REPLApplyShort(command.Substring(1));
            var found = REPLCommands.ContainsKey(command);
            if (found)
            {
                REPLCommands[command].Invoke(command);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(command)) OutAsException("Empty REPL Command");
                else OutAsException($"REPL Command \"{command}\" not found. Please, check list of commands with >help.");
            }
        }
        #endregion

        public static void OutREPLInfo()
        {
            Console.Title = $"{REPLVersion.Name}";
            OutCurrentVersion();
            Console.WriteLine($"Maded by Alexandr Kotov. Pegasus is cool!");
            Console.WriteLine($"get REPL comands - >h");
        }

        private static Parser REPLParser;
        private static bool REPLLoop;
        private static ExecutionContext REPLContext;
        public static void StartREPL(ExecutionContext myExecutionContext = null)
        {
            OutREPLInfo();

            REPLParser = new SLThree.Parser();
            REPLContext = myExecutionContext ?? new ExecutionContext();
            REPLLoop = true;
            while (REPLLoop)
            {
                Console.Write(">>> ");
                var code = Console.ReadLine();
                if (code.StartsWith(">"))
                {
                    REPLCommand(code);
                }
                else
                {
                    try
                    {
                        var st = REPLParser.ParseScript(code);
                        var value = st.GetValue(REPLContext);
                        OutAsOutput(value);
                    }
                    catch (Exception e)
                    {
                        OutException(e);
                    }
                }
            }
        }
        #endregion

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
