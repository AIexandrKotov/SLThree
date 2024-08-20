using System.IO;
using System.Text;

namespace SLThree.HTMLCreator
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var input = File.ReadAllText("input.slt", Encoding.UTF8);
            var x = Parser.This.ParseScript(input);
            File.WriteAllText("output.html", new HTMLCreator().GetHTMLCode(x is StatementList sl ? sl.Statements : new BaseStatement[1] { x }));
        }
    }
}
