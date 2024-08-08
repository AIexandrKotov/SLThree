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

        private static string CurrentSearchOpt = "pubs";
        private static Dictionary<string, Func<Type, bool>> Searchs = new Dictionary<string, Func<Type, bool>>()
        {
            { "all", x => true },
            { "pubs", x => x.IsPublic || x.IsNestedPublic },
            { "privs", x => !(x.IsPublic || x.IsNestedPublic)},
        };

        public enum VisitorResult
        {
            Not,
            Defined,
            Noticed,
        }

        public static class VisitorInfoBuilder
        {
            public static IEnumerable<(string, VisitorResult[])> ResearchTypes(Type interface_type, string visit_method_name, IEnumerable<Type> target_types, IEnumerable<Type> visitors)
            {
                Func<Type, bool> predicate;
                if (Searchs.TryGetValue(CurrentSearchOpt, out var result))
                    predicate = result;
                else predicate = x => true;

                foreach (var target in target_types.Where(predicate))
                {
                    var AVdefine = interface_type.GetMethods().Any(x => x.Name == visit_method_name && x.GetParameters()[0].ParameterType == target);
                    yield return (target.Name, 
                        Enumerable.Concat(Enumerable.Repeat(AVdefine ? VisitorResult.Defined : VisitorResult.Not, 1),
                        visitors.Select(
                            t => t.GetMethods()
                                  .Any(x => x.Name == visit_method_name 
                                            && x.GetParameters()[0].ParameterType == target
                                            && x.DeclaringType != typeof(AbstractVisitor))
                                 ? VisitorResult.Defined
                                 : (t.GetCustomAttributes<VisitorNoticeAttribute>().Select(x => (x.MethodName, x.MethodArg)).Contains((visit_method_name, target)) ? VisitorResult.Noticed : VisitorResult.Not)
                        )
                    ).ToArray());
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

            public static IEnumerable<(string, ConsoleColor, VisitorResult[])> ResearchAllTypes(IEnumerable<Type> visitors)
            {
                foreach (var x in ResearchTypes(typeof(IStatementVisitor), "VisitStatement", Statements, visitors))
                    yield return (x.Item1, ConsoleColor.Magenta, x.Item2);
                foreach (var x in ResearchTypes(typeof(IExpressionVisitor), "VisitExpression", Expressions, visitors))
                    yield return (x.Item1, ConsoleColor.Cyan, x.Item2);
                foreach (var x in ResearchTypes(typeof(IConstraintVisitor), "VisitConstraint", Constraints, visitors))
                    yield return (x.Item1, ConsoleColor.Yellow, x.Item2);
            }
        }

        public static void set_search(string s) => CurrentSearchOpt = s;

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

            var defs = new Dictionary<VisitorResult, string>
            {
                { VisitorResult.Not, "not" },
                { VisitorResult.Defined, "defined" },
                { VisitorResult.Noticed, "noticed" },
            };
            var colors = new Dictionary<VisitorResult, ConsoleColor>
            {
                { VisitorResult.Not, ConsoleColor.Red },
                { VisitorResult.Defined, ConsoleColor.Green },
                { VisitorResult.Noticed, ConsoleColor.Cyan },
            };
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
            show(typeof(TreeViewer), typeof(XmlViewer), typeof(TemplateMethod.GenericFinder), typeof(NETGenerator));
        }

        public static void temp()
        {
            show(typeof(GenericMethod.GenericFinder), typeof(TemplateMethod.GenericFinder));
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
