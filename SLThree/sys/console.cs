using SLThree.Extensions;
using System;
using System.Linq;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class console
    {
        public static ContextWrap color = new ContextWrap(new ExecutionContext(false, false, Enum.GetNames(typeof(ConsoleColor)).ToDictionary(x => x, x => Enum.Parse(typeof(ConsoleColor), x)).ToLocals()));
        public static string readln() => Console.ReadLine();
        public static void write(object o) => Console.Write(o);
        public static void writeln() => Console.WriteLine();
        public static void writeln(object o) => Console.WriteLine(o);
        public static ConsoleColor fore { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public static ConsoleColor back { get => Console.BackgroundColor; set => Console.BackgroundColor = value; }
    }
#pragma warning restore IDE1006 // Стили именования
}