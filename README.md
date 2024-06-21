# SLThree

.NET script programming language.

For more information about language: [slt-lang.github.io](https://slt-lang.github.io)

### Powerful REPL with useful commands

![image](https://github.com/AIexandrKotov/SLThree/assets/44296606/1ed80829-6428-4db7-b956-ee2968b77d2a)

### Embedding
SLThree may be runned in any .NET language. Minimal version for .NET Framework is 4.7.1, for .NET Standard is 2.1.

Here code example for C#:

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
            Console.WriteLine("return 2 + 2 * 2;".RunScript().ReturnedValue);

            //Uwrapping method
            var tc = "x = 3; y = 5; sum = (a, b) => a + b;".RunScript().Unwrap<TC>();
            Console.WriteLine(tc.Sum());

            //[Experimental] Compilation:
            var tc2 = "sum2 = (new using jit).opt(sum2 = (i32 x, i32 y): i32 => x + y, self).CreateDelegate(@System.Func<i32, i32, i32>);".RunScript().Unwrap<TC2>();
            Console.WriteLine(tc2.sum2(35, 65));
        }
    }
}

```

### LANG and REPL compatibility

Starting from language version 0.8.0, REPL no longer supports multiple versions at once and is built for each language update.

| REPL version    | LANG version    |
|-----------------|-----------------|
| 2.0.0           | 0.7.0           |
| 1.*             | 0.2.0 — 0.6.0   |

### Download
[![stable](https://img.shields.io/badge/REPL_stable-2.0.0-00cc00)](https://github.com/AIexandrKotov/SLThree/releases/tag/0.7.0) [![stable](https://img.shields.io/badge/LANG_exp-0.7.0-ccaa00)](https://github.com/AIexandrKotov/SLThree/releases/tag/0.7.0)