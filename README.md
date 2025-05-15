# SLThree
[![stable](https://img.shields.io/badge/Interpretator-0.9.0--alpha.1090-ffcc00)](https://github.com/AIexandrKotov/SLThree/releases/tag/0.9.0-alpha.1090)
[![stable](https://img.shields.io/badge/NuGet-0.9.0--alpha.1090-ffcc00)](https://www.nuget.org/packages/SLThree/0.9.0-alpha.1090#readme-body-tab)

## Overview

.NET script programming language.

Information about language: [slt-lang.ru](https://slt-lang.ru)


## REPL / Interpretator

[Download REPL](https://github.com/AIexandrKotov/SLThree/releases) and try `>-h` command!

![Image](https://github.com/user-attachments/assets/cff38d83-10e0-4c2d-9f4a-c28e5f6885fd)

### [Undocummented] Projects support

The REPL is also used for script projects execution:
```
slt.exe main.slt --project
```

All .slt files are executed in an undefined order. Therefore, use lazy constructs in all files (except the entry file): methods and contexts.

Add `--nonrecursive` if you do not want files to be executed from subfolders.

## How to use in my code?
SLThree may be runned in any .NET language. Minimal version for .NET Framework is 4.7.1, for .NET Standard is 2.1 (.NET Core 3+)

### Add SLThree nuget package:

```
dotnet add package SLThree --version 0.9.0-alpha.1090
```

### And use any of these ways to integrate SLThree with your application

```CSharp
using SLThree;
using SLThree.Intergration;
using SLThree.Language;

namespace HowToUseInMyCode
{
    public class Program
    {
        /// <summary>
        /// ScriptBuilder way.
        /// The easiest way to execute, out of the box, supporting files, directories, resources.
        /// The order of definition sets the order of execution.
        /// </summary>
        public static void BuilderWay()
        {
            var ret = new Integrator.ScriptBuilder()
                .WithCode("x = y = 10;")
                .Compile(new Parser())
                .Run();

            Console.WriteLine(ret.wrap);
        }

        /// <summary>
        /// Dictionary way.
        /// You can use the variables of the executed script directly
        /// </summary>
        public static void DictionaryWay()
        {
            Console.WriteLine(string.Join("\n",
                "x = 2; y = 3;"
                .RunScript()
                .LocalVariables.GetAsDictionary().Select(x => $"{x.Key}: {x.Value}")
            ));
        }

        /// <summary>
        /// Context.ReturnedValue method.
        /// The script body will be executed in the context.
        /// Any context has ReturnedValue!
        /// </summary>
        public static void ReturnedValueWay()
        {
            Console.WriteLine(
                "return 2 + 2 * 2;"
                .RunScript().ReturnedValue
            );
        }

        public class ClassForUnwrapping
        {
            public long x, y;
            public Method sum;
            public object Sum() => sum.Invoke(x, y);
        }

        /// <summary>
        /// Uwrapping method.
        /// Any context can be unwrapped into a class by filling in public fields or properties with context variables.<br/>
        /// If you want to customize this behavior, look at the <see href="https://github.com/AIexandrKotov/SLThree/blob/aea808f82586102e4733b620d55ae489f72974e5/SLThree/Wrapper.cs#L199">
        /// existing attributes</see>.
        /// </summary>
        public static void UnwrappingWay()
        {
            var tc = @"x = 3;
                       y = 5;
                       sum = (a, b) => a + b;"
                .RunScript()
                .Unwrap<ClassForUnwrapping>();
            Console.WriteLine(tc.Sum());
        }

        /// <summary>
        /// RunScript extensions uses the DT class, but since the SLThree object model allows you to use other languages, you need to specify the parser to use.
        /// </summary>
        public static void Main()
        {
            BuilderWay();

            DotnetEnvironment.DefaultParser = new SLThree.Language.Parser();

            DictionaryWay();
            ReturnedValueWay();
            UnwrappingWay();

            Console.ReadLine();
        }
    }
}

```
### You can find this project [here](https://github.com/AIexandrKotov/SLThree/tree/master/SampleProjects/HowToUseInMyCode)
#### In the future, more convenient script execution methods will appear.