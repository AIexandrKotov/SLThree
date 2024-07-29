using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.sys;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static SLThree.TemplateMethod.GenericInfo;

namespace SLThree
{
    public class TemplateMethod : Method
    {
        public abstract class ConstraintDefinition : BaseExpression
        {
            public ConstraintDefinition(SourceContext context) : base(context) { }
            public ConstraintDefinition(bool priority, SourceContext context) : base(priority, context) { }

            public override object GetValue(ExecutionContext context) => throw new NotSupportedException();
            public abstract Constraint GetConstraint(string current_template, ExecutionContext context);
        }
        public class NameConstraintDefinition : ConstraintDefinition
        {
            public BaseExpression Name;

            public NameConstraintDefinition(BaseExpression name, SourceContext context) : base(context)
            {
                Name = name;
            }

            public NameConstraintDefinition(BaseExpression name, bool priority, SourceContext context) : base(priority, context)
            {
                Name = name;
            }

            public Constraint TakeConstraint(object o)
            {
                switch (o)
                {
                    case Constraint constraint:
                        return constraint;
                    case ClassAccess access:
                        return new ConcrecteTypeConstraint(access.Name, SourceContext.CloneCast());
                    case Type type:
                        return new ConcrecteTypeConstraint(type, SourceContext.CloneCast());
                    default:
                        return (Constraint)o;
                }
            }
            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                if (Name is NameExpression name)
                {
                    var v = context.LocalVariables.GetValue(name.Name);
                    if (v.Item2 == -1)
                    {
                        switch (name.Name)
                        {
                            case "any":
                                return new AnyConstraint(SourceContext.CloneCast());
                            case "value":
                                return new ValueConstraint(SourceContext.CloneCast());
                            case "name":
                                return new NameConstraint(SourceContext.CloneCast());
                            case "type":
                                return new TypeConstraint(SourceContext.CloneCast());
                            case "expr":
                                return new ExprConstraint(SourceContext.CloneCast());
                            case "code":
                                return new CodeConstraint(SourceContext.CloneCast());
                            default:
                                {
                                    var type = name.Name.ToType();
                                    if (type == null) throw new RuntimeError($"Constraint {name.Name} not found", SourceContext);
                                    else return new ConcrecteTypeConstraint(type, SourceContext.CloneCast());
                                }
                        }
                    }
                    else return TakeConstraint(v.Item1);
                }
                else if (Name is MemberAccess memberAccess)
                {
                    var val = memberAccess.GetValue(context);
                    if (val == null)
                    {
                        var name2 = memberAccess.ToString();
                        var type = name2.ToType();
                        if (type == null) throw new RuntimeError($"Constraint {name2} not found", SourceContext);
                        else return new ConcrecteTypeConstraint(type, SourceContext.CloneCast());
                    }
                    else return TakeConstraint(val);
                }
                throw new RuntimeError($"Constraint {Name} not found", SourceContext);
            }

            public override string ExpressionToString() => Name.ToString();
            public override object Clone() => new NameConstraintDefinition(Name.CloneCast(), PrioriryRaised, SourceContext.CloneCast());
        }
        public class FunctionConstraintDefinition : ConstraintDefinition
        {
            public BaseStatement Statement;
            public FunctionConstraintDefinition(BaseStatement statement, SourceContext context) : base(context)
            {
                Statement = statement;
            }

            public FunctionConstraintDefinition(BaseStatement statement, bool priority, SourceContext context) : base(priority, context)
            {
                Statement = statement;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new FunctionConstraint(new Method("constraint", new string[1] { current_template }, new StatementList(new BaseStatement[1] { Statement }, Statement.SourceContext), new TypenameExpression[1], null, context.wrap, true, false, true, new BaseExpression[0]), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"=> {Statement}";

            public override object Clone() => new FunctionConstraintDefinition(Statement.CloneCast(), PrioriryRaised, SourceContext.CloneCast());
        }
        public class CombineConstraintDefinition : ConstraintDefinition
        {
            public ConstraintDefinition Left, Right;

            public CombineConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, SourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public CombineConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, bool priority, SourceContext context) : base(priority, context)
            {
                Left = left;
                Right = right;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new CombineConstraint(Left.GetConstraint(current_template, context), Right.GetConstraint(current_template, context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"{Left} + {Right}";

            public override object Clone() => new CombineConstraintDefinition(Left.CloneCast(), Right.CloneCast(), PrioriryRaised, SourceContext.CloneCast());
        }
        public class IntersectionConstraintDefinition : ConstraintDefinition
        {
            public ConstraintDefinition Left, Right;

            public IntersectionConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, SourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public IntersectionConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, bool priority, SourceContext context) : base(priority, context)
            {
                Left = left;
                Right = right;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new IntersectionConstraint(Left.GetConstraint(current_template, context), Right.GetConstraint(current_template, context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"{Left} | {Right}";

            public override object Clone() => new IntersectionConstraintDefinition(Left.CloneCast(), Right.CloneCast(), PrioriryRaised, SourceContext.CloneCast());
        }

        public abstract class Constraint : ICloneable
        {
            public bool PrioriryRaised { get; set; }
            public SourceContext SourceContext { get; set; }
            public Constraint() { }
            public Constraint(SourceContext context) => SourceContext = context;
            public Constraint(bool priority, SourceContext context) => (SourceContext, PrioriryRaised) = (context, priority);
            public override string ToString() => PrioriryRaised ? $"({ConstraintToString()})" : ConstraintToString();
            public abstract string ConstraintToString();
            public abstract bool Applicable(object target);
            public abstract object Clone();
        }

        public class AnyConstraint : Constraint
        {
            public AnyConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"any";

            public override bool Applicable(object target) => true;

            public override object Clone() => new AnyConstraint(SourceContext.CloneCast());
        }
        public class ValueConstraint : Constraint
        {
            public ValueConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"value";

            public override bool Applicable(object target)
            {
                return !(target is BaseExpression);
            }

            public override object Clone() => new ValueConstraint(SourceContext.CloneCast());
        }
        public class NameConstraint : Constraint
        {
            public NameConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"name";

            public override bool Applicable(object target)
            {
                return target is NameExpression;
            }

            public override object Clone() => new NameConstraint(SourceContext.CloneCast());
        }
        public class TypeConstraint : Constraint
        {
            public TypeConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"type";

            public override bool Applicable(object target)
            {
                return target is TypenameExpression;
            }

            public override object Clone() => new TypeConstraint(SourceContext.CloneCast());
        }
        public class ConcrecteTypeConstraint : Constraint
        {
            public readonly Type Type;
            public ConcrecteTypeConstraint(Type type, SourceContext context) : base(context) => Type = type;
            public override string ConstraintToString() => $"{Type.GetTypeString()}";

            public override bool Applicable(object target)
            {
                return target?.GetType().IsType(Type) ?? false;
            }

            public override object Clone() => new ConcrecteTypeConstraint(Type, SourceContext.CloneCast());
        }
        public class ExprConstraint : Constraint
        {
            public ExprConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"expr";

            public override bool Applicable(object target)
            {
                return target is BaseExpression;
            }

            public override object Clone() => new ExprConstraint(SourceContext.CloneCast());
        }
        public class CodeConstraint : Constraint
        {
            public CodeConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"code";

            public override bool Applicable(object target)
            {
                return target is BlockExpression;
            }

            public override object Clone() => new CodeConstraint(SourceContext.CloneCast());
        }
        public class CombineConstraint : Constraint
        {
            public Constraint Left;
            public Constraint Right;

            public CombineConstraint(Constraint left, Constraint right, SourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public override bool Applicable(object target)
            {
                return Left.Applicable(target) && Right.Applicable(target);
            }

            public override string ConstraintToString() => $"{Left} + {Right}";

            public override object Clone() => new CombineConstraint(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
        public class IntersectionConstraint : Constraint
        {
            public Constraint Left;
            public Constraint Right;

            public IntersectionConstraint(Constraint left, Constraint right, SourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }
            public override bool Applicable(object target)
            {
                return Left.Applicable(target) || Right.Applicable(target);
            }
            public override string ConstraintToString() => $"{Left} | {Right}";
            public override object Clone() => new IntersectionConstraint(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
        public class FunctionConstraint : Constraint
        {
            public FunctionConstraint(Method method, SourceContext context) : base(context)
            {
                Predicate = method;
            }
            public readonly Method Predicate;
            public override string ConstraintToString() => $"=> ...";

            public override bool Applicable(object target)
            {
                return Predicate.Invoke(target).Cast<bool>();
            }

            public override object Clone() => new FunctionConstraint(Predicate.CloneCast(), SourceContext.CloneCast());
        }
        public class CustomConstraint : Constraint
        {
            public string Name;
            public readonly Constraint Constraint;

            public CustomConstraint(string name, Constraint constraint, SourceContext context) : base(context)
            {
                Name = name;
                Constraint = constraint;
            }

            public override string ConstraintToString() => $"constraint {Name}: {Constraint}";

            public override bool Applicable(object target)
            {
                return Constraint.Applicable(target);
            }

            public override object Clone() => new CustomConstraint(Name, Constraint.CloneCast(), SourceContext.CloneCast());
        }

        public abstract class GenericInfo
        {
            public virtual object Placement { get; }
            /// <summary>
            /// Индекс шаблона (напр. для &lt;T1, T2&gt; T1 - 0, T2 - 1)
            /// </summary>
            public int GenericPosition;

            private static Dictionary<Type, Func<BaseExpression, int, GenericInfo>> GenericConstructors = 
                    typeof(GenericInfo)
                    .GetNestedTypes()
                    .Where(x => !x.GetInterfaces().Contains(typeof(IComplexedGenericInfo)))
                    .Select(x => (x, x.BaseType?.GetGenericArguments()[0]))
                    .ToDictionary(x => x.Item2, x =>
                    {
                        var method = new DynamicMethod($"Constructor${x.x.Name}", typeof(GenericInfo), new Type[] { typeof(BaseExpression), typeof(int) });
                        var constructor = x.x.GetConstructor(new Type[2] { x.Item2, typeof(int) });
                        var il = method.GetILGenerator();

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Isinst, x.x);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Newobj, constructor);
                        il.Emit(OpCodes.Ret);

                        return (Func<BaseExpression, int, GenericInfo>)method.CreateDelegate(typeof(Func<BaseExpression, int, GenericInfo>));
                    });

            public abstract ref BaseExpression GetPlacer();
            public virtual void MakeGeneric(BaseExpression expression) => GetPlacer() = expression;
            public void MakeValue(object any) => MakeGeneric(new StaticExpression(any));


            public static GenericInfo GetGenericInfo(BaseExpression expression, int pos)
            {
                var type = expression.GetType();
                if (GenericConstructors.TryGetValue(type, out var info))
                    return info.Invoke(expression, pos);
                else throw new RuntimeError($"Templates not supported in {type.GetTypeString()}", expression.SourceContext);
            }

            private interface IComplexedGenericInfo { }

            public class InvokeExpressionGeneric : GenericInfo<InvokeExpression>, IComplexedGenericInfo
            {
                public InvokeExpressionGeneric(InvokeExpression concrete, int position) : base(concrete, position) { }
                public override ref BaseExpression GetPlacer() => ref Concrete.Left;
            }
            public class BinaryOperatorGeneric : GenericInfo<BinaryOperator>, IComplexedGenericInfo
            {
                public bool IsRight;

                public BinaryOperatorGeneric(BinaryOperator concrete, int position) : base(concrete, position) { }
                
                public override void MakeGeneric(BaseExpression expression)
                {
                    if (IsRight) Concrete.Right = expression;
                    else Concrete.Left = expression;
                }

                public override ref BaseExpression GetPlacer()
                {
                    throw new NotImplementedException();
                }
            }
            public class UnaryOperatorGeneric : GenericInfo<UnaryOperator>
            {
                public UnaryOperatorGeneric(UnaryOperator concrete, int position) : base(concrete, position) { }

                public override ref BaseExpression GetPlacer() => ref Concrete.Left;
            }
        }
        public static class SupraGenericInfo
        {
            public class ExpressionGeneric : SupraGenericInfo<ExpressionStatement>
            {
                public ExpressionGeneric(ExpressionStatement concrete, int position) : base(concrete, position)
                {
                }

                public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
            }
            public class ReturnGeneric : SupraGenericInfo<ReturnStatement>
            {
                public ReturnGeneric(ReturnStatement concrete, int position) : base(concrete, position)
                {
                }

                public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
            }
        }
        public abstract class SupraGenericInfo<T> : GenericInfo where T: BaseStatement
        {
            public T Concrete;
            public sealed override object Placement => Concrete;
            public SupraGenericInfo(T concrete, int position)
            {
                Concrete = concrete;
                GenericPosition = position;
            }
        }
        public abstract class GenericInfo<T> : GenericInfo where T : BaseExpression
        {
            public T Concrete;
            public sealed override object Placement => Concrete;
            public GenericInfo(T concrete, int position)
            {
                Concrete = concrete;
                GenericPosition = position;
            }
        }
        public class GenericMethodSignature : GenericInfo
        {
            public TemplateMethod Method;
            public int position;
            public override void MakeGeneric(BaseExpression expression)
            {
                if (position == -1) Method.ReturnType = expression as TypenameExpression;
                else Method.ParamTypes[position] = expression as TypenameExpression;
            }

            public override ref BaseExpression GetPlacer()
            {
                throw new NotImplementedException();
            }
        }
        internal class GenericFinder : AbstractVisitor
        {
            public string[] Generics;
            public TemplateMethod Method;

            public List<GenericInfo> Infos = new List<GenericInfo>();

            public override void VisitStatement(ExpressionStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (statement is ExpressionStatement expr && expr.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new SupraGenericInfo.ExpressionGeneric(expr, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(ReturnStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (statement is ReturnStatement expr && expr.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new SupraGenericInfo.ReturnGeneric(expr, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitExpression(TypenameExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Typename is NameExpression name && (name.Name == Generics[i]))
                    {
                        if (Executables.Count == 0)
                        {
                            Infos.Add(new GenericMethodSignature()
                            {
                                Method = Method,
                                GenericPosition = i,
                                position = Method.ReturnType == expression ? -1 : Array.IndexOf(Method.ParamTypes, expression)
                            });
                            break;
                        }
                        var place = Executables[Executables.Count - 1] as BaseExpression;
                        if (place == expression) place = Executables[Executables.Count - 2] as BaseExpression;
                        switch (place)
                        {
                            /*case TypenameExpression typename:
                                for (var j = 0; j < typename.Generics.Length; j++)
                                    if (typename.Generics[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                    {
                                        Infos.Add(new Typename(typename, i, j));
                                    }
                                break;
                            case InvokeGenericExpression invokeGeneric:
                                for (var j = 0; j < invokeGeneric.GenericArguments.Length; j++)
                                    if (invokeGeneric.GenericArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                    {
                                        Infos.Add(new TemplateInfo.InvokeGenericExpression(invokeGeneric, i, j));
                                    }
                                break;
                            case ReflectionExpression reflection:
                                if (reflection.MethodGenericArguments != null)
                                    for (var j = 0; j < reflection.MethodGenericArguments.Length; j++)
                                        if (reflection.MethodGenericArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                        {
                                            Infos.Add(new Reflection(reflection, i, true, j));
                                        }
                                if (reflection.MethodArguments != null)
                                    for (var j = 0; j < reflection.MethodArguments.Length; j++)
                                        if (reflection.MethodArguments[j].Typename?.Cast<NameExpression>().Name == Generics[i])
                                        {
                                            Infos.Add(new Reflection(reflection, i, false, j));
                                        }
                                break;*/
                            default:
                                Infos.Add(GetGenericInfo(place, i));
                                break;
                        }
                    }
                    base.VisitExpression(expression);
                }
            }

            public override void VisitExpression(BinaryOperator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Left is NameExpression name && name.Name == Generics[i]) 
                    {
                        Infos.Add(new BinaryOperatorGeneric(expression, i) { IsRight = false });
                    }
                    if (expression.Right is NameExpression name2 && name2.Name == Generics[i])
                    {
                        Infos.Add(new BinaryOperatorGeneric(expression, i) { IsRight = true });
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(UnaryOperator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new UnaryOperatorGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(InvokeExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new InvokeExpressionGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void Visit(Method method)
            {
                if (Method != method) return;
                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                base.Visit(method);
            }

            public override void Visit(TemplateMethod method)
            {
                if (Method != method) return;
                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                base.Visit(method);
            }

            public static List<GenericInfo> FindAll(TemplateMethod method)
            {
                var gf = new GenericFinder();
                gf.Generics = method.Generics.ConvertAll(x => x.Item1.Name);
                gf.Method = method;
                gf.Visit(method);
                return gf.Infos;
            }
        }

        public enum Apply
        {
            Expression = 0,
            Value = 1,
            Type = 2,
            Runtime = 0xFF,
        }

        public TemplateMethod(string name, string[] paramNames, StatementList statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ContextWrap definitionPlace, bool @implicit, bool recursive, bool without_params, BaseExpression[] default_values, (NameExpression, Constraint)[] generics) : base(name, paramNames, statements, paramTypes, returnType, definitionPlace, @implicit, recursive, without_params, default_values)
        {
            Generics = generics;
            GenericsInfo = GenericFinder.FindAll(this);
            DefinitionParamTypes = ParamTypes.CloneArray();
            DefinitionReturnType = ReturnType.CloneCast();
        }

        public readonly TypenameExpression[] DefinitionParamTypes;
        public readonly TypenameExpression DefinitionReturnType;

        public Method MakeGenericMethod((Apply, object)[] args)
        {
            for (var i = 0; i < Generics.Length; i++)
                if (!Generics[i].Item2.Applicable(args[i].Item2))
                    throw new RuntimeError($"{args[i].Item2.GetType().GetTypeString()} {args[i].Item2} doesn't fit the \"{Generics[i].Item2}\"", Generics[i].Item2.SourceContext);
            foreach (var x in GenericsInfo)
            {
                var pos = x.GenericPosition;
                switch (args[pos].Item1)
                {
                    case Apply.Expression:
                        x.MakeGeneric(args[pos].Item2 as BaseExpression);
                        break;
                    case Apply.Type:
                        x.MakeGeneric(args[pos].Item2 as TypenameExpression);
                        break;
                    case Apply.Value:
                        x.MakeValue(args[pos].Item2);
                        break;
                    case Apply.Runtime:
                        {
                            switch (args[pos].Item2)
                            {
                                case TypenameExpression typename:
                                    x.MakeGeneric(typename);
                                    break;
                                case BaseExpression expression:
                                    x.MakeGeneric(expression);
                                    break;
                                case Type type:
                                    x.MakeGeneric(new TypenameGenericPart() { Type = type });
                                    break;
                                default:
                                    x.MakeValue(args[pos].Item2);
                                    break;
                            }
                            break;
                        }
                }
            }
                
            return base.CloneWithNewName(Name);
        }

        public readonly (NameExpression, Constraint)[] Generics;
        public override string ToString()
        {
            var sb = new StringBuilder();
            var unnamed = Name == DefaultMethodName;
            if (slt.is_abstract(Statements))
                sb.Append("abstract ");
            else
            {
                if (Recursive)
                    sb.Append("recursive ");
                if (!Implicit)
                    sb.Append("explicit ");
            }
            if (!unnamed)
                sb.Append(Name);
            sb.Append($"<{Generics.Select(x => $"{x.Item1}: {x.Item2}").JoinIntoString(", ")}>");
            sb.Append($"({DefinitionParamTypes.ConvertAll(x => x?.ToString() ?? "any").JoinIntoString(", ")})");
            sb.Append($": {DefinitionReturnType?.ToString() ?? "any"}");
            return sb.ToString();
        }

        public List<GenericInfo> GenericsInfo;

        public override Method CloneWithNewName(string name)
        {
            return new TemplateMethod(name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes?.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, DefaultValues.CloneArray(), Generics.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())))
            {
                Abstract = Abstract
            };
        }
    }
}
