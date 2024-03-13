using slt.sys;
using SLThree;
using SLThree.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace slt
{
    internal static class Program
    {
        static Program()
        {
#if NET6_0_OR_GREATER
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            EncodingAliases = new Dictionary<string, Encoding>()
            {
                { "utf-8", Encoding.UTF8 },
                { "utf-16", Encoding.Unicode },
                { "unicode", Encoding.Unicode },
                { "ansi", Encoding.GetEncoding(1250) },
            };
#endif
            InitSLThreeAssemblyInfo();
            SupportingFeatures();
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
        private static Dictionary<string, Encoding> EncodingAliases
#if NETFRAMEWORK
            = new Dictionary<string, Encoding>()
        {
            { "utf-8", Encoding.UTF8 },
            { "utf-16", Encoding.Unicode },
            { "unicode", Encoding.Unicode },
            { "ansi", Encoding.GetEncoding(1250) },
        }
#endif
            ;
        internal static Assembly SLThreeAssembly;
        private static SLTVersion.Reflected SLThreeVersion;
        private static REPLVersion.Reflected SLTREPLVersion;
        private static SortedDictionary<string, string[]> SLThreeVersions;
        private static string[] Specification;
        private static bool HasArgument(string arg)
            => RunArguments.HasArgument(arg, ShortCommands);
        private static bool TryGetArgument(string arg, out string value, Func<string> not_found = null)
            => RunArguments.TryGetArgument(arg, out value, not_found, ShortCommands);
        private static string GetArgument(string arg) => RunArguments.GetArgument(arg, ShortCommands);
        private static void InitSLThreeAssemblyInfo()
        {
            SLThreeAssembly = Assembly.GetAssembly(typeof(SLTVersion));
            SLThreeVersion = new SLTVersion.Reflected();
            SLTREPLVersion = new REPLVersion.Reflected();
            var sltver = SLThreeAssembly.GetType("SLTVersion");
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
                catch (ArgumentException)
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
            Console.Write($" {SLTREPLVersion.VersionWithoutRevision} ");
            Console.ResetColor();
            Console.Write("rev ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{SLTREPLVersion.Revision}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(SLTVersion.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {SLThreeVersion.VersionWithoutRevision} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{SLTVersion.Edition} ");
            Console.ResetColor();
            Console.Write("rev ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{SLThreeVersion.Revision}");
            Console.ResetColor();
        }

        public static void OutVersion(string version)
        {
            if (SLThreeVersions.TryGetValue(version.Replace("last", SLThreeVersion.VersionWithoutRevision), out var data))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(data.JoinIntoString("\n"));
                Console.ResetColor();
            }
            else OutAsWarning($"Version {version} not found");
        }

        public static void OutREPLVersion(string version)
        {
            if (DocsIntergration.REPLVersionsData.TryGetValue(version.Replace("last", SLTREPLVersion.VersionWithoutRevision), out var data))
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
                Console.WriteLine(e.GetType().Name + ": " + e.Message);
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

        private static object SafeFromContext(object value)
        {
            if (value is ExecutionContext.ContextWrap wrap)
                return $"context {wrap.pred.Name}";
            if (value is null)
                return "null";
            else return value;
        }

        private static object GetChoosersOutput(object value)
        {
            if (value == null) return value;
            if (value is ITuple tuple)
            {
                var xvalue = tuple.Enumerate(); value =
                    $"({(xvalue.Count() <= repl.count ? xvalue.Enumerate().Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") : xvalue.Enumerate().Take(repl.count).Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") + "...")})";
            }
            if (value is IList list) value =
                    $"[{(list.Count <= repl.count ? list.Enumerate().Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") : list.Enumerate().Take(repl.count).Select(x => SafeFromContext(GetOutput(x))).JoinIntoString(", ") + "...")}]";
            if (value is IDictionary dict) value =
                    $"{{{(dict.Count <= repl.count ? dict.Keys.Enumerate().Select(x => $"{SafeFromContext(GetOutput(x))}: {SafeFromContext(GetOutput(dict[x]))}").JoinIntoString(", ") : dict.Keys.Enumerate().Take(repl.count).Select(x => $"{SafeFromContext(GetOutput(x))}: {SafeFromContext(GetOutput(dict[x]))}").JoinIntoString(", ") + "...")}}}";
            var type = value.GetType();
            if (value is IChooser)
            {
                if (value is IChanceChooser chanceChooser)
                {
                    var values = chanceChooser.Values;
                    value = values.Count > repl.count
                        ? $"({values.Take(repl.count).Select(x => $"{x.Item1}: {x.Item2.ToDynamicPercents()}").JoinIntoString(" \\ ")}...)"
                        : $"({values.Select(x => $"{x.Item1}: {x.Item2.ToDynamicPercents()}").JoinIntoString(" \\ ")})";
                }
                else if (value is IEqualchanceChooser chooser)
                {
                    var values = chooser.Values;
                    value = values.Count > repl.count
                        ? $"({values.Take(repl.count).JoinIntoString(" \\ ")}...)"
                        : $"({values.JoinIntoString(" \\ ")})";
                }
            }
            return value;
        }

        public static object GetOutput(object value)
        {
            if (value is string) value = $"\"{value}\"";
            else if (value is Type type) value = type.GetTypeString();
            else value = GetChoosersOutput(value);
            if (value is string str && str.Length > repl.max_output) return str.Substring(0, repl.max_output - 3) + "...";
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
        public static ExecutionContext InvokeFile(string filename, ExecutionContext context, Encoding encoding = null, bool show_result = true)
        {
            var parser = new SLThree.Parser();
            var executionContext = context;
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

        private static void SupportingFeatures()
        {
            RegisterNewSystemTypes();
            ExecutionContext.ContextWrap.Decoration = GetOutput;
        }

        private static void RegisterNewSystemTypes()
        {
            foreach (var x in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.FullName.StartsWith("slt.sys.") && !x.Name.StartsWith("<")).ToDictionary(x => x.Name, x => x))
            {
                SLThree.sys.slt.sys_types.Add(x.Key, x.Value);
            }
            SLThree.sys.slt.registred.Add(typeof(Program).Assembly);
        }

        #region REPL Commands

        //table -1 - not table, table -2 - auto table, >0 - auto with minimum
        public static void OutUsingClasses(ExecutionContext context, int table = -1)
        {
            var local_usings0 = context.LocalVariables.GetAsDictionary()
                .Where(x => x.Value != null && (x.Value is MemberAccess.ClassAccess))
                .Select(x => new KeyValuePair<string, string>(x.Key, (x.Value as MemberAccess.ClassAccess)?.Name?.GetTypeString() ?? "undefined"));
            var local_usings = new Dictionary<string, List<string>>();
            foreach (var x in local_usings0)
            {
                if (local_usings.ContainsKey(x.Value))
                    local_usings[x.Value].Add(x.Key);
                else local_usings[x.Value] = new List<string>() { x.Key };
            }

            if (local_usings.Count == 0) return;
            OutAsWarning($"--- CLASSES ---");
            var max_variable_name = table;
            if (table < 0) max_variable_name = 0;
            if (local_usings.Count > 0 && table != -1)
            {
                max_variable_name = Math.Max(max_variable_name, local_usings.Max(x => x.Key.Length));
            }
            foreach (var x in local_usings)
            {
                Console.Write("    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(x.Key.PadLeft(max_variable_name));
                Console.ResetColor();
                Console.Write(" as ");
                var many = false;
                foreach (var alias in x.Value)
                {
                    if (many)
                    {
                        Console.ResetColor();
                        Console.Write(", ");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(alias);
                    many = true;
                }
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void OutLocalMethods(ExecutionContext context, int table = -1)
        {
            Dictionary<string, (string, string[], bool, string[])> local_methods = context.LocalVariables.GetAsDictionary()
                .Where(x => x.Value != null && (x.Value is Method || x.Value is MethodInfo))
                .ToDictionary(x => x.Key, x =>
                {
                    if (x.Value is GenericMethod gmethod)
                    {
                        return (gmethod.DefinitionReturnType?.ToString() ?? "any", gmethod.DefinitionParamTypes.Select(t => t?.ToString() ?? "any").ToArray(), true, gmethod.Generics.ConvertAll(a => a.Name));
                    }
                    if (x.Value is Method method)
                    {
                        return (method.ReturnType?.ToString() ?? "any", method.ParamTypes.Select(t => t?.ToString() ?? "any").ToArray(), true, new string[0]);
                    }
                    else if (x.Value is MethodInfo info)
                    {
                        var ret_gens = info.IsGenericMethodDefinition ? info.GetGenericArguments().ConvertAll(a => a.Name) : new string[0];
                        return (info.ReturnType == typeof(void) ? "void" : info.ReturnType.GetTypeString(), info.GetParameters().Select(y => y.ParameterType.GetTypeString()).ToArray(), false, ret_gens);
                    }
                    return default;
                });
            if (local_methods.Count == 0) return;
            OutAsWarning($"--- METHODS ---");
            var max_ret_type = table;
            var max_method_name = table;
            var max_generics = table;
            //var max_method_args = table;
            if (table < 0)
            {
                max_ret_type = 0;
                max_method_name = 0;
                max_generics = 0;
                //max_method_args = 0;
            }
            if (local_methods.Count > 0 && table != -1)
            {
                max_ret_type = Math.Max(max_ret_type, local_methods.Max(x => x.Value.Item1?.Length ?? 0));
                max_method_name = Math.Max(max_method_name, local_methods.Max(x => x.Key.Length));
                max_generics = Math.Max(max_generics, local_methods.Max(x => x.Value.Item4.Length == 0 ? 0 : (x.Value.Item4.Sum(a => a.Length) + (x.Value.Item4.Length - 1) * 2)));
                //max_method_args = Math.Max(max_method_args, local_methods.Max(x => x.Value.Item2.Sum(m => m.Length) + 2 * (x.Value.Item2.Length - 1)));
            }

            var local_methods_nongeneric = local_methods.Where(x => x.Value.Item4.Length == 0);
            var local_methods_generic = local_methods.Where(x => x.Value.Item4.Length != 0);
            foreach (var x in local_methods_nongeneric)
            {
                Console.Write("    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(x.Value.Item1.PadLeft(max_ret_type));
                Console.ResetColor();
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(x.Key.PadRight(max_method_name));
                Console.ResetColor();
                Console.Write("(");
                var tnext = false;
                foreach (var t in x.Value.Item2)
                {
                    if (tnext)
                    {
                        Console.ResetColor();
                        Console.Write(", ");
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(t);
                    tnext = true;
                }
                Console.ResetColor();
                Console.Write(") ");
                Console.WriteLine();
            }
            foreach (var x in local_methods_generic)
            {
                Console.Write("    ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(x.Value.Item1.PadLeft(max_ret_type));
                Console.ResetColor();
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(x.Key.PadRight(max_method_name));
                Console.ResetColor();
                Console.Write("<");
                var tnext = false;
                foreach (var t in x.Value.Item4)
                {
                    if (tnext)
                    {
                        Console.ResetColor();
                        Console.Write(", ");
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(t);
                    tnext = true;
                }
                Console.ResetColor();
                Console.Write(">");
                if (max_generics != 0)
                    Console.Write("".PadRight(max_generics - x.Value.Item4.Sum(a => a.Length) - (x.Value.Item4.Length - 1) * 2));
                Console.Write("(");
                tnext = false;
                foreach (var t in x.Value.Item2)
                {
                    if (tnext)
                    {
                        Console.ResetColor();
                        Console.Write(", ");
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(t);
                    tnext = true;
                }
                Console.ResetColor();
                Console.Write(") ");
                Console.WriteLine();
            }
        }

        public static void OutLocalVariables(ExecutionContext context, int table = -1, bool typed = false)
        {
            var local_variables = context.LocalVariables.GetAsDictionary()
                .Where(x => x.Value == null || !(x.Value is MethodInfo || x.Value is Method || x.Value is MemberAccess.ClassAccess))
                .ToDictionary(x => x.Key, x => x.Value);
            if (local_variables.Count == 0) return;
            OutAsWarning($"--- VARIABLES ---");
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
                Console.ResetColor();
                if (typed)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write((types[x.Key] ?? "").PadRight(max_variable_type));
                    Console.Write(" ");
                    Console.ResetColor();
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(x.Key.PadLeft(max_variable_name));
                Console.Write(" = ");
                Console.ResetColor();
                var output = x.Value is ExecutionContext.ContextWrap wrap ? $"context {wrap.pred.Name}" : GetOutput(x.Value)?.ToString() ?? "null";
                Console.Write(output);
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static void OutLocals(ExecutionContext context, int table = -1, bool typed = false)
        {
            OutUsingClasses(context, table);
            OutLocalMethods(context, table);
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
            { "-f", "--run-file" },
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
        public static ExecutionContext GetNewREPLContext()
        {
            var context = new ExecutionContext();
            return context;
        }

        public static bool ExtendedCommands(string command)
        {
            string[] Splitter(string str)
            {
                var ret = new List<string>();
                var current = new StringBuilder();

                const int state_whitespace = 0;
                const int state_any = 1;
                const int state_string = 2;

                var state = state_whitespace;

                foreach (var c in str)
                {
                    if (state == state_string)
                    {
                        if (c == '"')
                        {
                            ret.Add(current.ToString());
                            current.Clear();
                            state = state_whitespace;
                        }
                        else
                        {
                            current.Append(c);
                            continue;
                        }
                    }
                    else
                    {
                        if (char.IsWhiteSpace(c))
                        {
                            if (state == state_whitespace) continue;
                            else
                            {
                                ret.Add(current.ToString());
                                current.Clear();
                                state = state_whitespace;
                            }
                        }
                        else
                        {
                            if (c == '"')
                            {
                                if (state == state_whitespace)
                                {
                                    state = state_string;
                                    continue;
                                }
                                else current.Append(c);
                            }
                            else
                            {
                                if (state == state_whitespace)
                                {
                                    state = state_any;
                                    current.Append(c);
                                    continue;
                                }
                                else current.Append(c);
                            }
                        }
                    }
                }

                if (current.Length > 0) ret.Add(current.ToString());

                return ret.ToArray();
            }

            var wrds = Splitter(command);
            wrds = Array.ConvertAll(wrds, x => x.StartsWith("-") && !x.StartsWith("--") ? x.ReplaceAll(ShortREPLCommands) : x);

            var any_executed = false;

            if (wrds.HasArgument("-l", ShortREPLCommands))
            {
                var typed = wrds.HasArgument("--typed");
                var context =
                    wrds.HasArgument("--global")
                    ? ExecutionContext.global.pred
                        : (wrds.TryGetArgument("--context", out var vname)
                            ? SLThree.sys.slt.eval(vname).TryCastRef<ExecutionContext.ContextWrap>()?.pred ?? REPLContext
                            : REPLContext)
                        ;
                if (wrds.TryGetArgument("--table", out var tablestr, () => (-2).ToString()) && int.TryParse(tablestr, out var table))
                {
                    OutLocals(context, table, typed);
                }
                else
                {
                    OutLocals(context, -1, typed);
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
            if (wrds.TryGetArgument("-f", out var runfile_path, null, ShortREPLCommands))
            {
                var encoding = default(Encoding);
                if (wrds.TryGetArgument("-e", out var encodingstr, () => null, ShortCommands))
                    encoding = GetEncoding(encodingstr);

                var context = default(ExecutionContext);
                if (wrds.TryGetArgument("--in", out var runfile_incontext, () => "self"))
                {
                    var ocontext = SLThree.sys.slt.eval(new ExecutionContext.ContextWrap(REPLContext), runfile_incontext);
                    switch (ocontext)
                    {
                        case ExecutionContext cc:
                            context = cc;
                            break;
                        case ExecutionContext.ContextWrap wrap:
                            context = wrap.pred;
                            break;
                    }
                }
                else context = REPLContext;

                if (context != null)
                {
                    InvokeFile(runfile_path, context, encoding, wrds.HasArgument("--show"));
                }
                else
                {
                    OutAsException($"`{runfile_incontext}` is not context");
                }
                any_executed = true;
            }
            if (wrds.HasArgument("-r", ShortREPLCommands))
            {
                var onlynull = wrds.HasArgument("--onlynull");

                if (onlynull)
                {
                    var old = REPLContext.LocalVariables.NamedIdenificators.Count;
                    REPLContext.LocalVariables.ClearNulls();
                    var @new = REPLContext.LocalVariables.NamedIdenificators.Count;
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
            Console.Write($"{SLTREPLVersion.VersionWithoutRevision}");
            Console.ResetColor();
            Console.Write(" | ");
            Console.ForegroundColor = ConsoleColor.Cyan;
#if NETFRAMEWORK
            Console.WriteLine(".NET Framework 4.7.1");
#else
#if NET8_0_OR_GREATER
            Console.WriteLine(".NET 8.0");
#else
#if NET7_0_OR_GREATER
            Console.WriteLine(".NET 7.0");
#else
#if NET6_0_OR_GREATER
            Console.WriteLine(".NET 6.0");
#endif
#endif
#endif
#endif
            Console.ResetColor();
        }

        public static void OutREPLInfo()
        {
            Console.Title = $"{REPLVersion.FullName}";
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
        private static Subparser REPLSubparser;
        private static bool REPLLoop;
        internal static ExecutionContext REPLContext;
        private static bool REPLPerfomance = false;
        private static Stopwatch ParsingStopwatch = new Stopwatch();
        private static Stopwatch ExecutingStopwatch = new Stopwatch();
        public static void StartREPL(ExecutionContext myExecutionContext = null)
        {
            OutREPLInfo();

            REPLParser = new SLThree.Parser();
            REPLSubparser = new Subparser();
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
                if (REPLSubparser.State == Subparser.SubparserState.Ready)
                    Console.Write(">>> ");
                else if (REPLSubparser.State == Subparser.SubparserState.WaitingText)
                    Console.Write("... " + new string(' ', REPLSubparser.Tabs * 4));
                try
                {
                    var state = REPLSubparser.Parse(Console.ReadLine());
                    if (state == Subparser.SubparserState.WaitingText) continue;
                }
                catch (Exception e)
                {
                    REPLSubparser.Clear();
                    OutException(e);
                    continue;
                }
                Console.ResetColor();
                var code = REPLSubparser.CurrentInput.ToString();
                REPLSubparser.CurrentInput.Clear();
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
                        if (REPLPerfomance) ExecutingStopwatch = Stopwatch.StartNew();
                        REPLContext.PrepareToInvoke();
                        var value = st.GetValue(REPLContext);
                        if (REPLPerfomance) ExecutingStopwatch.Stop();
                        cancelationToken = false;
                        Console.ResetColor();
                        OutAsOutput(value);
                        if (REPLPerfomance)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"Computed in {(ExecutingStopwatch.Elapsed + ParsingStopwatch.Elapsed).TotalMilliseconds} ms " +
                                $"(Parse: {ParsingStopwatch.Elapsed.TotalMilliseconds} ms, Exec: {ExecutingStopwatch.Elapsed.TotalMilliseconds} ms)");
                            Console.ResetColor();
                        }
                    }
                    catch (TargetInvocationException e)
                    {
                        OutException(e.InnerException);
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
