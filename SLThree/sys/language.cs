using SLThree.Extensions;
using SLThree.Native;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public static IEnumerable<Type> GetAncestorsInclude(Type type)
        {
            yield return type;
            type = type.BaseType;
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static class VisitorInfoBuilder
        {

            public static IEnumerable<(string, bool[])> ResearchTypes(Type interface_type, string visit_method_name, IEnumerable<Type> target_types, IEnumerable<Type> visitors)
            {
                foreach (var target in target_types)
                {
                    var AVdefine = interface_type.GetMethods().Any(x => x.Name == visit_method_name && x.GetParameters()[0].ParameterType == target);
                    yield return (target.Name, Enumerable.Concat(Enumerable.Repeat(AVdefine, 1), visitors.Select(t => t.GetMethods().Any(x => x.Name == visit_method_name && x.GetParameters()[0].ParameterType == target && x.DeclaringType != typeof(AbstractVisitor)))).ToArray());
                }
            }

            static VisitorInfoBuilder()
            {
                (Statements, Expressions, Constraints) = GetTargetTypes();
            }

            public static readonly Type[] Statements, Expressions, Constraints;

            public static (Type[], Type[], Type[]) GetTargetTypes()
            {
                var types = SLThree.GetTypes();
                var statements = types.Where(x => GetAncestors(x).Contains(typeof(BaseStatement)));
                var expressions = types.Where(x => GetAncestors(x).Contains(typeof(BaseExpression)));
                var constraints = types.Where(x => GetAncestorsInclude(x).Contains(typeof(TemplateMethod.ConstraintDefinition)));
                expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(Literal)));
                expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(Special)));
                expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(BinaryOperator)));
                expressions = expressions.Where(x => !GetAncestors(x).Contains(typeof(UnaryOperator)));
                expressions = expressions.Where(x => !GetAncestorsInclude(x).Contains(typeof(TemplateMethod.ConstraintDefinition)));

                return (statements.ToArray(), expressions.ToArray(), constraints.ToArray());
            }

            public static IEnumerable<(string, ConsoleColor, bool[])> ResearchAllTypes(IEnumerable<Type> visitors)
            {
                foreach (var x in ResearchTypes(typeof(IStatementVisitor), "VisitStatement", Statements, visitors))
                    yield return (x.Item1, ConsoleColor.Magenta, x.Item2);
                foreach (var x in ResearchTypes(typeof(IExpressionVisitor), "VisitExpression", Expressions, visitors))
                    yield return (x.Item1, ConsoleColor.Cyan, x.Item2);
                foreach (var x in ResearchTypes(typeof(IConstraintVisitor), "VisitConstraint", Constraints, visitors))
                    yield return (x.Item1, ConsoleColor.Yellow, x.Item2);
            }
        }

        public static void show(params Type[] targets)
        {
            var columnnames = targets.Select(x => x.Name.Where(c => char.IsUpper(c)).JoinIntoString("") + "def").ToList();
            columnnames.Insert(0, "Element name");
            columnnames.Insert(1, "AVdef");

            columnnames = columnnames.ConvertAll(x => x.PadRight(Math.Max(7, x.Length)));

            var infos = VisitorInfoBuilder.ResearchAllTypes(targets).ToList();
            infos.Sort((x, y) => (string.Compare(x.Item1, y.Item1)));
            var max = infos.Max(x => x.Item1.Length);
            columnnames[0] = columnnames[0].PadRight(max);

            var defs = new Dictionary<bool, string> { { false, "not" }, { true, "defined" } };
            var colors = new Dictionary<bool, ConsoleColor> { { false, ConsoleColor.Red }, { true, ConsoleColor.Green } };
            Console.ResetColor();


            Console.WriteLine(columnnames.JoinIntoString(" │ "));
            foreach (var x in infos)
            {
                Console.ForegroundColor = x.Item2;
                Console.Write(x.Item1.PadRight(columnnames[0].Length));
                Console.ResetColor();
                for (var i = 0; i < columnnames.Count - 1; i++)
                {
                    Console.Write(" │ ");
                    Console.ForegroundColor = colors[x.Item3[i]];
                    Console.Write(defs[x.Item3[i]].PadRight(7));
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public static void show()
        {
            show(typeof(TreeViewer), typeof(XmlViewer), typeof(NETGenerator));
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
