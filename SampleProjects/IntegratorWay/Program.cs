using SLThree.Intergration;
using SLThree.Language;

namespace IntegratorWay
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var envBuilder = new Integrator.ScriptBuilder();

            var nestedBuilder = new Integrator.ScriptBuilder()
                .WithFile("output.slt")
                .WithResource("IntegratorWay.resource.slt");

            var ret = envBuilder
                .WithCode("x = 2; return x + x * x;")
                .WithCode("y = self.unwrap().ReturnedValue; return y * 1000;")
                .AddBuilder(nestedBuilder)
                .Compile(new Parser())
                .Run();


            Console.WriteLine(ret.ReturnedValue);
            Console.WriteLine("output: " + (ret.LocalVariables.GetValue("output").Item1?.ToString() ?? "null"));
            Console.WriteLine("resource: " + (ret.LocalVariables.GetValue("resource").Item1?.ToString() ?? "null"));
            Console.ReadLine();
        }
    }
}
