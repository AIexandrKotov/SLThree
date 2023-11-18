﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SLThree;
using SLThree.Extensions;
using System.Reflection;

namespace TestSuite
{
    public static class Program
    {
        public static List<string> ErrorLog = new List<string>();

        private static bool current_assert = true;
        private static int current_assert_id = 1;
        public static void Assert(ExecutionContext.ContextWrap context, BaseLexem lexem)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("");
                Console.Write($"{current_assert_id++, 6}  ");
                if (lexem.GetValue(context.pred).Cast<bool>())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"SUCCESS ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($" {lexem}    ");
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine($"at {lexem.SourceContext.ToStringWithoutFile()}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($" FAILED ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write($" at {lexem.SourceContext.ToStringWithoutFile()}  ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{lexem}");
                    throw new RuntimeError("Assertion exception", lexem.SourceContext);
                }
            }
            catch (Exception e)
            {
                current_assert = false;
                ErrorLog.Add($"FAILED {lexem} as {lexem.SourceContext} ===> {e}");
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
                context.LocalVariables.SetValue("ASSERT", ((Action<ExecutionContext.ContextWrap, BaseLexem>)Assert).Method);


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

        static string removable_parsing = Path.GetFullPath("..\\test\\parsing\\");
        public static void ParsingTests()
        {
            Console.WriteLine(">>> Parsing Tests");
            foreach (var filename in Directory.GetFiles("..\\test\\parsing", "*.slt", SearchOption.AllDirectories))
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

        static string removable_executing = Path.GetFullPath("..\\test\\executing\\");
        public static void ExecutingTests()
        {
            Console.WriteLine(">>> Executing Tests");
            foreach (var filename in Directory.GetFiles("..\\test\\executing", "*.slt", SearchOption.AllDirectories))
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

        public static int Main(string[] args)
        {
            Console.Title = "SLThree Test Suite";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>> SLThree Test Suite");
            TypeofLexem.RegistredAssemblies.Add(typeof(Program).Assembly);
            ParsingTests();
            ExecutingTests();
            File.WriteAllLines("testsuite.log", ErrorLog.ToArray());
            if (ErrorLog.Count > 0) return 1;
            else return 0;
        }
    }
}