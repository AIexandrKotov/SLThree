using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SLThree;
using SLThree.Extensions;

namespace slt
{
    internal class Program
    {
        private static readonly Dictionary<string, string> ShortCommands = new Dictionary<string, string>()
        {
            { "-s", "--specification" },
            { "-v", "--version" },
            { "-a", "--allversions" },
            { "-h", "--help" },
        };

        public static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            args = Array.ConvertAll(args, x =>
            {
                var ret = x;
                foreach (var s in ShortCommands)
                    ret = ret.Replace(s.Key, s.Value);
                return ret;
            });

            if (args.Length == 0)
            {
                Console.Title = $"{SLTVersion.Name} {SLTVersion.VersionWithoutRevision} interpreter";
                Console.WriteLine($"{SLTVersion.Name} {SLTVersion.VersionWithoutRevision} by {SLTVersion.Author}");
                Console.WriteLine($"Revision {SLTVersion.Revision} build {TimeZoneInfo.ConvertTimeFromUtc(new DateTime(SLTVersion.LastUpdate), TimeZoneInfo.Local).ToString("dd.MM.yyyy HH:mm")}");
                Console.WriteLine($"SLThree based on Pegasus parser generator");

                var parser = new SLThree.Parser();
                var executionContext = new ExecutionContext();
                while (true)
                {
                    Console.Write(">>> ");
                    var code = Console.ReadLine();
                    if (code == "quit()") return;
                    try
                    {
                        var st = parser.Parse(code);
                        Console.WriteLine(st.GetValue(executionContext) ?? "null");
                    }
                    catch (Exception e)
                    {
                        var old = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
                        Console.WriteLine(e.ToString());
#else
                    Console.WriteLine(e.GetType().FullName + ": " + e.Message);
#endif
                        Console.ForegroundColor = old;
                    }
                }
            }
            if (args.Contains("--version"))
            {
                var ind = Array.FindIndex(args, x => x == "--version");
                if (args.Length > ind + 1)
                {
                    if (args[ind + 1] == "last")
                    {
                        Console.WriteLine(DocsIntergration.VersionsData[SLTVersion.VersionWithoutRevision].JoinIntoString("\n"));
                    }
                    else if (DocsIntergration.VersionsData.TryGetValue(args[ind + 1], out var data))
                    {
                        Console.WriteLine(data.JoinIntoString("\n"));
                    }
                    else Console.WriteLine($"Version {args[ind + 1]} not found");
                }
                else Console.WriteLine("Empty version argument");
            }
            if (args.Contains("--allversions"))
            {
                foreach (var entry in DocsIntergration.VersionsData)
                {
                    Console.WriteLine($"==> {entry.Key}");
                    Console.WriteLine(entry.Value.JoinIntoString("\n"));
                }
            }
            if (args.Contains("--specification"))
            {
                Console.WriteLine(DocsIntergration.Specification.JoinIntoString("\n"));
            }
            if (args.Contains("--help"))
            {
                Console.WriteLine(DocsIntergration.Help.JoinIntoString("\n"));
            }
        }
    }
}
