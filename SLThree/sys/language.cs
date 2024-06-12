using SLThree.Native;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class language
    {
        public static Assembly SLThree = typeof(language).Assembly;
        public static IEnumerable<Type> GetAncestors(Type type)
        {
            type = type.BaseType;
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static void show()
        {
            var types = SLThree.GetTypes();
            var statements = types.Where(x => GetAncestors(x).Contains(typeof(BaseStatement)));
            var expressions = types.Where(x => GetAncestors(x).Contains(typeof(BaseExpression)));
            expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(Literal)));
            expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(Special)));
            expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(BinaryOperator)));
            expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(UnaryOperator)));

            //name, AV define, TV define, XMLV define
            var infos = new List<(string, bool, bool, bool, bool, bool)>();
            foreach (var statement in statements)
            {
                var AVdefine = typeof(IStatementVisitor).GetMethods().Any(x => x.Name == "VisitStatement" && x.GetParameters()[0].ParameterType == statement);
                var TVdefine = typeof(TreeViewer).GetMethods().Any(x => x.Name == "VisitStatement" && x.GetParameters()[0].ParameterType == statement && x.DeclaringType != typeof(AbstractVisitor));
                var XMLdefine = typeof(XmlViewer).GetMethods().Any(x => x.Name == "VisitStatement" && x.GetParameters()[0].ParameterType == statement && x.DeclaringType != typeof(AbstractVisitor));
                var NETdefine = typeof(NETGenerator).GetMethods().Any(x => x.Name == "VisitStatement" && x.GetParameters()[0].ParameterType == statement && x.DeclaringType != typeof(AbstractVisitor));
                infos.Add((statement.Name, false, AVdefine, TVdefine, XMLdefine, NETdefine));
            }
            foreach (var expression in expressions)
            {
                var AVdefine = typeof(IExpressionVisitor).GetMethods().Any(x => x.Name == "VisitExpression" && x.GetParameters()[0].ParameterType == expression);
                var TVdefine = typeof(TreeViewer).GetMethods().Any(x => x.Name == "VisitExpression" && x.GetParameters()[0].ParameterType == expression && x.DeclaringType != typeof(AbstractVisitor));
                var XMLdefine = typeof(XmlViewer).GetMethods().Any(x => x.Name == "VisitExpression" && x.GetParameters()[0].ParameterType == expression && x.DeclaringType != typeof(AbstractVisitor));
                var NETdefine = typeof(NETGenerator).GetMethods().Any(x => x.Name == "VisitExpression" && x.GetParameters()[0].ParameterType == expression && x.DeclaringType != typeof(AbstractVisitor));
                infos.Add((expression.Name, true, AVdefine, TVdefine, XMLdefine, NETdefine));
            }
            infos.Sort((x, y) => (string.Compare(x.Item1, y.Item1)));
            var nameident = infos.Max(x => x.Item1.Length);
            var defs = new Dictionary<bool, string> { { false, "not" }, { true, "defined" } };
            var colors = new Dictionary<bool, ConsoleColor> { { false, ConsoleColor.Red }, { true, ConsoleColor.Green } };
            Console.ResetColor();
            Console.WriteLine($"{"Element name".PadRight(nameident)} │  AVdef  │  TVdef  │  XMLdef │ NETdef  ");
            foreach (var x in infos)
            {
                Console.ForegroundColor = x.Item2 ? ConsoleColor.Cyan : ConsoleColor.Magenta;
                Console.Write(x.Item1.PadRight(nameident));
                Console.ResetColor();
                Console.Write(" │ ");
                Console.ForegroundColor = colors[x.Item3];
                Console.Write(defs[x.Item3].PadRight(7));
                Console.ResetColor();
                Console.Write(" │ ");
                Console.ForegroundColor = colors[x.Item4];
                Console.Write(defs[x.Item4].PadRight(7));
                Console.ResetColor();
                Console.Write(" │ ");
                Console.ForegroundColor = colors[x.Item5];
                Console.Write(defs[x.Item5].PadRight(7));
                Console.ResetColor();
                Console.Write(" │ ");
                Console.ForegroundColor = colors[x.Item6];
                Console.Write(defs[x.Item6].PadRight(7));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
