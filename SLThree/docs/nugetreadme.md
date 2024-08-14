# SLThree

A simple and easily embedded programming language for the .NET platform. Dynamic typing, lightweight reflection, this language is convenient for describing data and behavior.

For more information about syntax: [slt-lang.github.io](https://slt-lang.github.io)

### Embedding
Code example for C#:

```CSharp
using System;
using SLThree;
using SLThree.Embedding;

namespace TestSLThree
{
    public class Program
    {
        public class TC
        {
            public int x, y;
            public Method sum;
            public object Sum() => sum.Invoke(x, y);
        }

        public class TC2
        {
            public Func<int, int, int> sum2;
        }

        static void Main(string[] args)
        {
            //Context.ReturnedValue method
            Console.WriteLine(
                "return 2 + 2 * 2;"
                .RunScript().ReturnedValue
            );

            //Uwrapping method
            var tc = "x = 3; y = 5; sum = (a, b) => a + b;"
                .RunScript()
                .Unwrap<TC>();
            Console.WriteLine(tc.Sum());

            //[Experimental] Compilation:
            var tc2 = @"(native sum2(i32 x, i32 y): i32 => x + y)
                        .CreateDelegate(@System.Func<i32, i32, i32>);"
                .RunScript()
                .Unwrap<TC2>();
            Console.WriteLine(tc2.sum2(35, 65));
        }
    }
}

```

### Download
[Releases](https://github.com/AIexandrKotov/SLThree/releases)