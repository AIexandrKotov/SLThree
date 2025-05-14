using SLThree;
using SLThree.Intergration;

namespace HowToUseInMyCode
{
    public class Program
    {
        /// <summary>
        /// Dictionary way
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
            DotnetEnvironment.DefaultParser = new SLThree.Language.Parser();

            DictionaryWay();
            ReturnedValueWay();
            UnwrappingWay();

            Console.ReadLine();
        }
    }
}
