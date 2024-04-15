using SLThree;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestSuite
{
    public static class Program
    {
        public static List<string> ErrorLog = new List<string>();

        private static bool current_assert = true;
        private static int current_assert_id = 1;
        public static void Log(object o)
        {
            ErrorLog.Add(o.ToString());
        }

        public static void Assert(ContextWrap context, BaseExpression expression)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("");
                Console.Write($"{current_assert_id++,6}  ");
                if (expression.GetValue(context.Context).Cast<bool>())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"SUCCESS ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($" {expression}    ");
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine($"at {expression.SourceContext.ToStringWithoutFile()}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" FAILED ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write($" at {expression.SourceContext.ToStringWithoutFile()}  ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{expression}");
                    throw new RuntimeError("Assertion exception", expression.SourceContext);
                }
            }
            catch (Exception e)
            {
                current_assert = false;
                ErrorLog.Add($"FAILED {expression} as {expression.SourceContext} ===> {e}");
            }
        }

        public static bool ParseTest(string filename)
        {
            try
            {
                Parser.This.ParseScript(File.ReadAllText(filename), filename);
                return true;
            }
            catch (SLTException e)
            {
                ErrorLog.Add(e.ToString());
                return false;
            }
            catch (Exception e)
            {
                ErrorLog.Add($"with {filename}: ");
                ErrorLog.Add(e.ToString());
                return false;
            }
        }

        public static bool ExecTest(string filename)
        {
            current_assert_id = 1;
            current_assert = true;
            try
            {
                var context = new ExecutionContext();
                context.LocalVariables.SetValue("ASSERT", ((Action<ContextWrap, BaseExpression>)Assert).Method);
                context.LocalVariables.SetValue("PATH", ((Func<string, string>)GetPath).Method);
                context.LocalVariables.SetValue("LOG", ((Action<string>)Log).Method);

                Parser.This.RunScript(File.ReadAllText(filename), filename, context);
                return current_assert;
            }
            catch (SLTException e)
            {
                ErrorLog.Add(e.ToString());
                return false;
            }
            catch (Exception e)
            {
                ErrorLog.Add($"with {filename}: ");
                ErrorLog.Add(e.ToString());
                return false;
            }
        }

        static string GetPath(string path) => from_solution ? path : Path.Combine("..\\..\\..", path);
        static readonly string removable_parsing = Path.GetFullPath(from_solution ? "test\\parsing\\" : "..\\..\\..\\test\\parsing\\");
        public static void ParsingTests()
        {
            Console.WriteLine(">>> Parsing Tests");
            foreach (var filename in Directory.GetFiles(from_solution ? "test\\parsing\\" : "..\\..\\..\\test\\parsing", "*.slt", SearchOption.AllDirectories))
            {
                if (ParseTest(filename))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"SUCCESS ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" FAILED ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Path.GetFullPath(filename).Replace(removable_parsing, ""));
            }
        }

        static readonly string removable_executing = Path.GetFullPath(from_solution ? "test\\executing\\" : "..\\..\\..\\test\\executing\\");
        public static void ExecutingTests()
        {
            Console.WriteLine(">>> Executing Tests");
            foreach (var filename in Directory.GetFiles(from_solution ? "test\\executing\\" : "..\\..\\..\\test\\executing", "*.slt", SearchOption.AllDirectories))
            {
                Console.WriteLine($">>> {Path.GetFullPath(filename).Replace(removable_executing, "")}");
                if (ExecTest(filename))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("result  ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"SUCCESS ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("result  ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" FAILED ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("------------------------------------");
            }
        }

        public static void ErrorsTests()
        {

        }

        private static bool from_solution = false;

        public static int Main(string[] args)
        {
            Console.Title = "SLThree Test Suite";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>> SLThree Test Suite");
            if (args.Contains("--from-solution"))
                from_solution = true;
            SLThree.sys.slt.registred.Add(typeof(Program).Assembly);
            ParsingTests();
            ExecutingTests();
            File.WriteAllLines("testsuite.log", ErrorLog.ToArray());
            if (ErrorLog.Count > 0) return 1;
            else return 0;
        }
    }
}
