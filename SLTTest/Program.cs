using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLThree;

namespace SLTTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var code = @"
max = 0;
x = 1;
while (x < 1000000) {
    index = x;
    step = 0;
    while (index > 1) {
        if (index % 2 == 0) {
            index /= 2;
            step += 1;
        }
        else {
            index = (3 * index + 1) / 2;
            step += 2;
        }
    }
    e = step;
    if (e > max) { max = e }
    x += 1;
}
";

            var parser = new Parser();
            var context = new ExecutionContext();
            var runnable = parser.Parse(code);
            var sw = Stopwatch.StartNew();
            runnable.GetValue(context);
            sw.Stop();

            Console.WriteLine(context.LocalVariables["max"]);   //524
            Console.WriteLine(sw.ElapsedMilliseconds);          //41700
            Console.ReadLine();
        }
    }
}
