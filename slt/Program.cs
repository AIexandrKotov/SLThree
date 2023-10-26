using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Pegasus.Common;
using SLThree;
using SLThree.Extensions;

namespace slt
{
    internal static class Program
    {
        static Program()
        {
            InitSLThreeAssemblyInfo();
        }

        #region Arguments
        private static readonly Dictionary<string, string> ShortCommands = new Dictionary<string, string>()
        {
            { "-s", "--specfile" },
            { "-v", "--version" },
            { "-d", "--diff" },
            { "-h", "--help" },
            { "-r", "--repl" },
            { "-e", "--encoding" },
            { "-V", "--repl-version" },
            { "-D", "--repl-diff" },
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
        private static bool HasArgument(string arg) 
            => RunArguments.HasArgument(arg, ShortCommands);
        private static bool TryGetArgument(string arg, out string value, Func<string> not_found = null)
            => RunArguments.TryGetArgument(arg, out value, not_found, ShortCommands);
        private static string GetArgument(string arg) => RunArguments.GetArgument(arg, ShortCommands);
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
        public static bool HasArgument(this string[] arguments, string arg, Dictionary<string, string> shorts = null)
            => arguments.Contains(shorts == null ? arg : arg.ReplaceAll(shorts));
        public static bool TryGetArgument(this string[] arguments, string arg, out string value, Func<string> not_found = null, Dictionary<string, string> shorts = null)
        {
            if (shorts != null) arg = arg.ReplaceAll(shorts);
            value = null;
            var ind = Array.FindIndex(arguments, x => x == arg);

            if (ind == -1) return false;
            if (arguments.Length > ind + 1 && !arguments[ind + 1].StartsWith("--"))
            {
                value = arguments[ind + 1];
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
        public static string GetArgument(this string[] arguments, string arg, Dictionary<string, string> shorts = null)
            => arguments.TryGetArgument(arg, out var value, null, shorts) ? value : null;

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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(data.JoinIntoString("\n"));
                Console.ResetColor();
            }
            else OutAsWarning($"Version {version} not found");
        }

        public static void OutREPLVersion(string version)
        {
            if (DocsIntergration.REPLVersionsData.TryGetValue(version.Replace("last", SLTVersionWithoutRevision), out var data))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(data.JoinIntoString("\n"));
                Console.ResetColor();
            }
            else OutAsWarning($"REPL version {version} not found");
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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                Console.ResetColor();
                i--;
            }
        }

        public static void OutDifferenceBy(string version)
        {
            foreach (var entry in SLThreeVersions.Reverse())
            {
                if (entry.Key.StartsWith(version)) return;
                Console.WriteLine($"==> {entry.Key}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                Console.ResetColor();
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
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                Console.ResetColor();
                i--;
            }
        }

        public static void OutREPLDifferenceBy(string version)
        {
            foreach (var entry in DocsIntergration.REPLVersionsData.Reverse())
            {
                if (entry.Key.StartsWith(version)) return;
                Console.WriteLine($"==> {entry.Key}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(entry.Value.JoinIntoString("\n"));
                Console.ResetColor();
            }
        }

        public static void OutSpecification()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Specification.JoinIntoString("\n"));
            Console.ResetColor();
        }

        public static void OutREPLHelp()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DocsIntergration.REPLHelp.JoinIntoString("\n"));
            Console.ResetColor();
        }

        public static void OutHelp()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DocsIntergration.Help.JoinIntoString("\n"));
            Console.ResetColor();
        }
        #endregion

        #endregion

        #region Universal outs
        private static bool out_extended_exceptions
#if DEBUG
            = true;
#else
            = false;
#endif
        public static void OutException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (out_extended_exceptions)
                Console.WriteLine(e.ToString());
            else
                Console.WriteLine(e.GetType().FullName + ": " + e.Message);
            Console.ResetColor();
        }

        public static void OutAsWarning(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        public static void OutAsException(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        private static object GetOutput(object value)
        {
            if (value is string) value = $"\"{value}\"";
            return value;
        }

        public static void OutAsOutput(object value)
        {
            if (value is null) return;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(GetOutput(value));
            Console.ResetColor();
        }
#endregion

#region Compiler and Interpreter
        public static ExecutionContext InvokeFile(string filename, Encoding encoding = null, bool show_result = true)
        {
            var parser = new SLThree.Parser();
            var executionContext = GetNewREPLContext();
            try
            {
                var st = parser.ParseScript(File.ReadAllText(filename, encoding ?? Encoding.UTF8), filename);
                var o = st.GetValue(executionContext);
                if (show_result) OutAsOutput(o);
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

        //table -1 - not table, table -2 - auto table, >0 - auto with minimum
        public static void OutUsingClasses(ExecutionContext context, int table = -1, bool typed = false)
        {
            var local_usings = context.LocalVariables
                .Where(x => x.Value != null && (x.Value is MemberAccess.ClassAccess))
                .ToDictionary(x => x.Key, x => x.Value?.Cast<MemberAccess.ClassAccess>().Name.FullName ?? "undefined");
            OutAsWarning($"--- {local_usings.Count} CLASSES ---");
            var max_variable_name = table;
            if (table < 0) max_variable_name = 0;
            if (local_usings.Count > 0 && table != -1)
            {
                max_variable_name = Math.Max(max_variable_name, local_usings.Max(x => x.Value.Length));
            }
            foreach (var x in local_usings)
            {
                Console.Write("    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(x.Value.PadLeft(max_variable_name));
                Console.ResetColor();
                Console.Write(" as ");
                Console.ForegroundColor = ConsoleColor.White;
                var output = x.Key.Trim().GetTypeString();
                Console.Write(output);
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void OutLocalMethods(ExecutionContext context, int table = -1, bool typed = false)
        {
            Dictionary<string, (string, string[], bool)> local_methods = context.LocalVariables
                .Where(x => x.Value != null && (x.Value is Method || x.Value is MethodInfo))
                .ToDictionary(x => x.Key, x =>
                {
                    if (x.Value is Method method)
                    {
                        return (string.Empty, method.ParamNames, true);
                    }
                    else if (x.Value is MethodInfo info)
                    {
                        return (info.ReturnType == typeof(void) ? "void" : info.ReturnType.GetTypeString(), info.GetParameters().Select(y => y.ParameterType.GetTypeString()).ToArray(), false);
                    }
                    return default;
                });
            OutAsWarning($"--- {local_methods.Count} METHODS ---");
            var max_ret_type = table;
            var max_method_name = table;
            //var max_method_args = table;
            if (table < 0)
            {
                max_ret_type = 0;
                max_method_name = 0;
                //max_method_args = 0;
            }
            if (local_methods.Count > 0 && table != -1)
            {
                max_ret_type = Math.Max(max_ret_type, local_methods.Max(x => x.Value.Item1?.Length ?? 0));
                max_method_name = Math.Max(max_method_name, local_methods.Max(x => x.Key.Length));
                //max_method_args = Math.Max(max_method_args, local_methods.Max(x => x.Value.Item2.Sum(m => m.Length) + 2 * (x.Value.Item2.Length - 1)));
            }
            foreach (var x in local_methods)
            {
                Console.Write("    ");
                if (x.Value.Item3)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("SLT".PadLeft(max_ret_type));
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(x.Value.Item1.PadLeft(max_ret_type));
                    Console.ResetColor();
                }
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(x.Key.PadRight(max_method_name));
                Console.ResetColor();
                Console.Write("(");
                Console.ForegroundColor = x.Value.Item3 ? ConsoleColor.Magenta : ConsoleColor.Cyan;
                Console.Write(x.Value.Item2.JoinIntoString(", "));
                Console.ResetColor();
                Console.Write(") ");
                Console.WriteLine();
            }
        }

        public static void OutLocalVariables(ExecutionContext context, int table = -1, bool typed = false)
        {
            var local_variables = context.LocalVariables
                .Where(x => x.Value == null || !(x.Value is MethodInfo || x.Value is Method || x.Value is MemberAccess.ClassAccess))
                .ToDictionary(x => x.Key, x => x.Value);
            OutAsWarning($"--- {local_variables.Count} VARIABLES ---");
            var max_variable_name = table;
            var max_variable_type = table;
            if (table < 0)
            {
                max_variable_name = 0;
                max_variable_type = 0;
            }
            var types = default(IDictionary<string, string>);
            if (typed)
            {
                types = local_variables.ToDictionary(x => x.Key, x => x.Value?.GetType().GetTypeString());
            }
            if (local_variables.Count > 0 && table != -1)
            {
                if (typed)
                {
                    max_variable_type = Math.Max(max_variable_type, types.Max(x => x.Value?.Length ?? 0));
                }
                max_variable_name = Math.Max(max_variable_name, local_variables.Keys.Max(x => x.Length));
            }
            foreach (var x in local_variables)
            {
                Console.Write("    ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(x.Key.PadLeft(max_variable_name));
                Console.ResetColor();
                if (typed)
                {
                    if (types[x.Key] == null)
                    {
                        Console.Write("".PadRight((table == -1 ? 0 : 2) + max_variable_type));
                    }
                    else
                    {
                        Console.Write(": ");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write((types[x.Key] ?? "").PadRight(max_variable_type));
                        Console.ResetColor();
                    }
                }
                Console.Write(" = ");
                Console.ResetColor();
                var output = GetOutput(x.Value)?.ToString() ?? "null";
                Console.Write(output);
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void OutLocals(ExecutionContext context, int table = -1, bool typed = false)
        {
            OutUsingClasses(context, table, typed);
            OutLocalMethods(context, table, typed);
            OutLocalVariables(context, table, typed);
        }

        public static Dictionary<string, string> ShortREPLCommands = new Dictionary<string, string>()
        {
            { "-s", "--specification" },
            { "-v", "--version" },
            { "-d", "--difference" },
            { "-V", "--repl-version" },
            { "-D", "--repl-difference" },


            { "-h", "--help" },
            { "-q", "--quit" },
            { "-c", "--clear" },
            { "-r", "--reset" },
            { "-H", "--conhelp" },
            { "-l", "--locals" },
            { "-p", "--perfomance" },
            { "-e", "--exex" },
        };
        public static Dictionary<string, Action> REPLCommands = new Dictionary<string, Action>()
        {
            { "--quit", () => REPLLoop = false },
            { "--clear", () => { Console.Clear(); OutREPLInfo(); } },
            { "--help", () => OutREPLHelp() },
            { "--conhelp", () => OutHelp() },
        };
        public static ExecutionContext GetNewREPLContext(bool update_global = true)
        {
            var context = new ExecutionContext();
            if (update_global) UpdateGlobalContext();
            return context;
        }
        public static void UpdateGlobalContext()
        {
            ExecutionContext.global.pred.LocalVariables["println"] = Method.Create<object>(Console.WriteLine);
            ExecutionContext.global.pred.LocalVariables["print"] = Method.Create<object>(Console.Write);
            ExecutionContext.global.pred.LocalVariables["readln"] = Method.Create(Console.ReadLine);
        }
        public static bool ExtendedCommands(string command)
        {
            var wrds = command.Split(new char[] { '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            wrds = Array.ConvertAll(wrds, x => x.StartsWith("-") && !x.StartsWith("--") ? x.ReplaceAll(ShortREPLCommands) : x);

            var any_executed = false;
            
            if (wrds.HasArgument("-l", ShortREPLCommands))
            {
                var typed = wrds.HasArgument("--typed");
                var context = wrds.HasArgument("--global") ? ExecutionContext.global.pred : REPLContext;
                if (wrds.TryGetArgument("--table", out var tablestr, () => (-2).ToString()) && int.TryParse(tablestr, out var table))
                {
                    OutLocals(context, table, typed);
                }
                else
                {
                    OutLocals(context, - 1, typed);
                }
                any_executed = true;
            }
            if (wrds.HasArgument("-p", ShortREPLCommands))
            {
                REPLPerfomance = !REPLPerfomance;
                OutAsWarning($"Counting perfomance is {REPLPerfomance}");
                any_executed = true;
            }
            if (wrds.HasArgument("-e", ShortREPLCommands))
            {
                out_extended_exceptions = !out_extended_exceptions;
                OutAsWarning($"Showing extended exceptions is {out_extended_exceptions}");
                any_executed = true;
            }
            if (wrds.HasArgument("-r", ShortREPLCommands))
            {
                var onlynull = wrds.HasArgument("--onlynull");

                if (onlynull)
                {
                    var old = REPLContext.LocalVariables.Count;
                    REPLContext.LocalVariables = REPLContext.LocalVariables.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
                    var @new = REPLContext.LocalVariables.Count;
                    OutAsWarning($"{old - @new} nulls deleted from context");
                }
                else
                {
                    REPLContext = GetNewREPLContext();
                    OutAsWarning("Context cleared");
                }
                any_executed = true;
            }

            if (wrds.HasArgument("-s", ShortREPLCommands))
            {
                OutSpecification();
                any_executed = true;
            }
            if (wrds.TryGetArgument("-v", out var version, null, ShortREPLCommands))
            {
                OutVersion(version);
                any_executed = true;
            }
            else if (wrds.HasArgument("-v", ShortREPLCommands))
            {
                OutCurrentVersion();
                any_executed = true;
            }
            if (wrds.TryGetArgument("-V", out var repl_version, null, ShortREPLCommands))
            {
                OutREPLVersion(repl_version);
                any_executed = true;
            }
            if (wrds.TryGetArgument("-d", out var last_versions, () => int.MaxValue.ToString(), ShortREPLCommands))
            {
                if (int.TryParse(last_versions, out var lasts)) OutDifference(lasts);
                else OutDifferenceBy(last_versions);
                any_executed = true;
            }
            if (wrds.TryGetArgument("-D", out var last_repl_versions, () => int.MaxValue.ToString(), ShortREPLCommands))
            {
                if (int.TryParse(last_repl_versions, out var lasts)) OutREPLDifference(lasts);
                else OutREPLDifferenceBy(last_repl_versions);
                any_executed = true;
            }

            foreach (var x in REPLCommands)
                if (wrds.HasArgument(x.Key))
                {
                    x.Value.Invoke();
                    any_executed = true;
                }

            return any_executed;
        }
        public static void REPLCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) OutAsWarning("Empty REPL Command");
            else if (!ExtendedCommands(command.Substring(1)))
            {
                OutAsWarning("Your request does nothing do");
            }
        }
#endregion

        public static void REPLShortVersion()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("SLThree REPL ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{REPLVersion.VersionWithoutRevision}");
            Console.ResetColor();
        }

        public static void OutREPLInfo()
        {
            Console.Title = $"{REPLVersion.Name}";
            REPLShortVersion();

            Console.Write("Maded by ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("Alexandr Kotov ");
            Console.ResetColor();
            Console.Write("using ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Pegasus");
            Console.ResetColor();

            Console.Write("Get ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("REPL commands ");
            Console.ResetColor();
            Console.Write("with ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">-h");
            Console.ResetColor();
        }

        private static Parser REPLParser;
        private static bool REPLLoop;
        private static ExecutionContext REPLContext;
        private static bool REPLPerfomance = false;
        private static Stopwatch ParsingStopwatch;
        private static Stopwatch ExecutinStopwatch;
        public static void StartREPL(ExecutionContext myExecutionContext = null)
        {
            OutREPLInfo();

            REPLParser = new SLThree.Parser();
            REPLContext = myExecutionContext ?? GetNewREPLContext();
            REPLLoop = true;

            bool cancelationToken = false;
            void cancelKeyPress(object o, ConsoleCancelEventArgs e)
            {
                if (!cancelationToken)
                {
                    e.Cancel = true;
                    REPLContext.Return();
                }
                cancelationToken = true;
            }

            Console.CancelKeyPress += cancelKeyPress;
            while (REPLLoop)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(">>> ");
                var code = Console.ReadLine();
                Console.ResetColor();
                if (string.IsNullOrWhiteSpace(code)) continue;
                if (code.StartsWith(">"))
                {
                    REPLCommand(code);
                }
                else
                {
                    try
                    {
                        if (REPLPerfomance) ParsingStopwatch = Stopwatch.StartNew();
                        var st = REPLParser.ParseScript(code);
                        if (REPLPerfomance) ParsingStopwatch.Stop();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        if (REPLPerfomance) ExecutinStopwatch = Stopwatch.StartNew();
                        var value = st.GetValue(REPLContext.PrepareToInvoke());
                        if (REPLPerfomance) ExecutinStopwatch.Stop();
                        cancelationToken = false;
                        Console.ResetColor();
                        OutAsOutput(value);
                        if (REPLPerfomance)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"Computed in {(ExecutinStopwatch.Elapsed + ParsingStopwatch.Elapsed).TotalMilliseconds} ms " +
                                $"(Parse: {ParsingStopwatch.Elapsed.TotalMilliseconds} ms, Exec: {ExecutinStopwatch.Elapsed.TotalMilliseconds} ms)");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception e)
                    {
                        OutException(e);
                    }
                }
            }
            Console.CancelKeyPress -= cancelKeyPress;
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
                    var context = InvokeFile(args[0], encoding, true);
                    StartREPL(context);
                }
                else InvokeFile(args[0], encoding, true);
            }
            else if (HasArgument("-r") || args.Length == 0) StartREPL();
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
