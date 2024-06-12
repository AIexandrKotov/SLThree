using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.Native
{
    public class NameCollector : AbstractVisitor
    {
        public readonly ExecutionContext definition_place;
        public readonly ExecutionContext temporary_method_context;
        int index;
        List<AbstractNameInfo> variables = new List<AbstractNameInfo>();
        Dictionary<NameExpression, AbstractNameInfo> variablesMap = new Dictionary<NameExpression, AbstractNameInfo>();
        List<Type> Returnings = new List<Type>();

        public NameCollector(Method method, ExecutionContext context)
        {
            definition_place = context;
            temporary_method_context = new ExecutionContext(definition_place);
            for (var i = 0; i < method.ParamNames.Length; i++)
            {
                var vname = new AbstractNameInfo()
                {
                    Index = i,
                    NameType = NameType.Parameter,
                    Name = method.ParamNames[i],
                    Type = (Type)method.ParamTypes[i].GetValue(context),
                };
                variables.Add(vname);
            }
        }

        public Type GetAutotype(BaseExpression expression, bool throws = true)
        {
            if (expression.GetType().Name.Contains("Literal"))
            {
                var bt = expression.GetType().BaseType;
                if (bt.IsGenericType && bt.GetGenericTypeDefinition() == typeof(Literal<>))
                    return bt.GetGenericArguments()[0];
            }
            if (expression is InterpolatedString) return typeof(string);
            //if (expression is NewExpression newExpression) return (Type)newExpression.Typename.GetValue(temporary_method_context);
            if (expression is NameExpression name)
            {
                var vname = variables.LastOrDefault(x => x.Name == name.Name);
                if (vname != null)
                {
                    variablesMap[name] = vname;
                    return vname.Type;
                }
            }

            if (throws) throw new ArgumentException($"Untyped variable {expression} at {expression.SourceContext}");
            return null;
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            //todo collect returning in other visitor
            //if (statement.Expression != null)
            //    Returnings.Add(GetAutotype(statement.Expression));
            base.VisitExpression(statement.Expression);
        }

        public override void VisitExpression(BinaryOperator expression)
        {
            if (expression is BinaryAssign assign)
            {
                var name = assign.Left as NameExpression;
                var vname = variables.LastOrDefault(x => x.Name == name.Name);
                if (vname != null)
                {
                    if (name.TypeHint != null)
                    {
                        vname = new AbstractNameInfo()
                        {
                            Name = name.Name,
                            NameType = NameType.Local,
                            Type = name.TypeHint?.GetStaticValue() ?? GetAutotype(assign.Right),
                            Index = index++,
                        };
                        variables.Add(vname);
                        variablesMap[name] = vname;
                    }
                    else variablesMap[name] = vname;
                }
                else
                {
                    vname = new AbstractNameInfo()
                    {
                        Name = name.Name,
                        NameType = NameType.Local,
                        Type = name.TypeHint?.GetStaticValue() ?? GetAutotype(assign.Right),
                        Index = index++,
                    };
                    variables.Add(vname);
                    variablesMap[name] = vname;
                }
                VisitExpression(expression.Right);
            }
            else base.VisitExpression(expression);
        }

        public override void VisitExpression(NameExpression expression)
        {
            var vname = variables.LastOrDefault(x => x.Name == expression.Name);
            if (vname != null)
                variablesMap[expression] = vname;
            else throw new ArgumentException($"Unknown variable {expression} at {expression.SourceContext}");
        }

        public static (List<AbstractNameInfo>, Dictionary<NameExpression, AbstractNameInfo>) Collect(Method method, ExecutionContext context)
        {
            var vc = new NameCollector(method, context);
            vc.Visit(method);
            return (vc.variables, vc.variablesMap);
        }
    }
}
