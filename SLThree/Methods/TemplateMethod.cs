using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.sys;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

            public Constraint GetNameConstraint(BaseExpression Name, string current_template, ExecutionContext context)
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
                                    return new ConcrecteTypeConstraint((Type)this.Name.GetValue(context), SourceContext.CloneCast());
                                }
                        }
                    }
                    return new ConcrecteTypeConstraint((Type)this.Name.GetValue(context), SourceContext.CloneCast());
                }
                else return new ConcrecteTypeConstraint((Type)this.Name.GetValue(context), SourceContext.CloneCast());
                throw new RuntimeError($"Constraint {Name} not found", SourceContext);
            }
            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                if (Name is TypenameExpression te && (te.Generics?.Length ?? 0) == 0 && !te.is_array)
                    return GetNameConstraint(te.Typename, current_template, context);
                else return new ConcrecteTypeConstraint((Type)Name.GetValue(context), SourceContext.CloneCast());
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
            public abstract bool Applicable(GenericMaking making, object target);
            public abstract GenericMakingConstraint GetMakingConstraint();
            public static GenericMakingConstraint CombineMakingConstraint(GenericMakingConstraint left, GenericMakingConstraint right)
            {
                return left & right;
            }
            public static GenericMakingConstraint IntersectionMakingConstraint(GenericMakingConstraint left, GenericMakingConstraint right)
            {
                return left | right;
            }
            public abstract object Clone();
        }
        public class AnyConstraint : Constraint
        {
            public AnyConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"any";

            public override bool Applicable(GenericMaking making, object target) => true;

            public override object Clone() => new AnyConstraint(SourceContext.CloneCast());

            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
        }
        public class ValueConstraint : Constraint
        {
            public ValueConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"value";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsValue);
            }

            public override object Clone() => new ValueConstraint(SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowValues;
        }
        public class NameConstraint : Constraint
        {
            public NameConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"name";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsName);
            }

            public override object Clone() => new NameConstraint(SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowNames;
        }
        public class TypeConstraint : Constraint
        {
            public TypeConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"type";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsType);
            }

            public override object Clone() => new TypeConstraint(SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowTypes;
        }
        public class ConcrecteTypeConstraint : Constraint
        {
            public readonly Type Type;
            public ConcrecteTypeConstraint(Type type, SourceContext context) : base(context) => Type = type;
            public override string ConstraintToString() => $"{Type.GetTypeString()}";

            public override bool Applicable(GenericMaking making, object target)
            {
                return target?.GetType().IsType(Type) ?? false;
            }

            public override object Clone() => new ConcrecteTypeConstraint(Type, SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
        }
        public class ExprConstraint : Constraint
        {
            public ExprConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"expr";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsExpression);
            }

            public override object Clone() => new ExprConstraint(SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowExpressions;
        }
        public class CodeConstraint : Constraint
        {
            public CodeConstraint(SourceContext context) : base(context) { }
            public override string ConstraintToString() => $"code";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsCode);
            }

            public override object Clone() => new CodeConstraint(SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowCode;
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

            public override bool Applicable(GenericMaking making, object target)
            {
                return Left.Applicable(making, target) && Right.Applicable(making, target);
            }

            public override string ConstraintToString() => $"{Left} + {Right}";

            public override object Clone() => new CombineConstraint(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => CombineMakingConstraint(Left.GetMakingConstraint(), Right.GetMakingConstraint());
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
            public override bool Applicable(GenericMaking making, object target)
            {
                return Left.Applicable(making, target) || Right.Applicable(making, target);
            }
            public override string ConstraintToString() => $"{Left} | {Right}";
            public override object Clone() => new IntersectionConstraint(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => IntersectionMakingConstraint(Left.GetMakingConstraint(), Right.GetMakingConstraint());
        }
        public class FunctionConstraint : Constraint
        {
            public FunctionConstraint(Method method, SourceContext context) : base(context)
            {
                Predicate = method;
            }
            public readonly Method Predicate;
            public override string ConstraintToString() => $"=> ...";

            public override bool Applicable(GenericMaking making, object target)
            {
                return Predicate.Invoke(target).Cast<bool>();
            }

            public override object Clone() => new FunctionConstraint(Predicate.CloneCast(), SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
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

            public override bool Applicable(GenericMaking making, object target)
            {
                return Constraint.Applicable(making, target);
            }

            public override object Clone() => new CustomConstraint(Name, Constraint.CloneCast(), SourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => Constraint.GetMakingConstraint();
        }

        public abstract class GenericInfo
        {
            public virtual object Placement { get; }
            /// <summary>
            /// Индекс шаблона (напр. для &lt;T1, T2&gt; T1 - 0, T2 - 1)
            /// </summary>
            public int GenericPosition;

            public TypenameExpression AsType(object any)
            {
                if (any is ClassAccess ca) return new TypenameGenericPart() { Type = ca.Name };
                return new TypenameGenericPart() { Type = (Type)any };
            }
            public NameExpression AsName(object any)
            {
                if (any is NameExpression expr) return expr;
                if (any is StringLiteral str) return new NameExpression((string)str.Value, str.SourceContext);
                return new NameExpression(any.ToString(), null);
            }
            public BaseExpression AsExpression(object any)
            {
                if (any is BaseExpression expr) return expr;
                throw new RuntimeError($"{any} is not expression", ContraitConstraint.TakeContext(Placement as ExecutionContext.IExecutable));
            }
            public BaseStatement AsStatement(object any)
            {
                if (any is BaseStatement st1) return st1;
                if (any is BlockExpression be) return new StatementList(be.Statements, be.SourceContext.CloneCast());
                throw new RuntimeError($"{any} is not code", ContraitConstraint.TakeContext(Placement as ExecutionContext.IExecutable));
            }
            public BaseExpression AsStatementList(BaseStatement statement)
            {
                if (statement is StatementList sl)
                    return new BlockExpression(sl.Statements, sl.SourceContext);
                return new BlockExpression(new BaseStatement[1] { statement }, statement.SourceContext);
            }

            public abstract void MakeValue(object any);
            public abstract void MakeType(TypenameExpression type);
            public abstract void MakeName(NameExpression name);
            public abstract void MakeExpression(BaseExpression expression);
            public abstract void MakeCode(BaseStatement statement);
            public void Make(GenericMaking making, object any)
            {
                switch (making)
                {
                    case GenericMaking.AsValue:
                        MakeValue(any);
                        return;
                    case GenericMaking.AsType:
                        MakeType(AsType(any));
                        return;
                    case GenericMaking.AsName:
                        MakeName(AsName(any));
                        return;
                    case GenericMaking.AsExpression:
                        MakeExpression(AsExpression(any)); 
                        return;
                    case GenericMaking.AsCode:
                        MakeCode(AsStatement(any));
                        return;
                }
            }
        }
        public abstract class GenericInfo<T> : GenericInfo where T : class
        {
            public T Concrete;
            public sealed override object Placement => Concrete;
            public GenericInfo(T concrete, int position)
            {
                Concrete = concrete;
                GenericPosition = position;
            }
        }
        public abstract class ExprGenericInfo<T> : GenericInfo<T> where T : BaseExpression
        {
            public ExprGenericInfo(T concrete, int position) : base(concrete, position) { }
        }
        public abstract class CodeGenericInfo<T> : GenericInfo<T> where T: BaseStatement
        {
            public CodeGenericInfo(T concrete, int position) : base(concrete, position) { }
        }
        public abstract class SameBehaviourExprGenericInfo<T> : ExprGenericInfo<T> where T : BaseExpression
        {
            public SameBehaviourExprGenericInfo(T concrete, int position) : base(concrete, position) { }

            public abstract ref BaseExpression GetPlacer();

            public override void MakeValue(object any)
            {
                GetPlacer() = new StaticExpression(any);
            }

            public override void MakeType(TypenameExpression type)
            {
                GetPlacer() = type;
            }

            public override void MakeName(NameExpression name)
            {
                GetPlacer() = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                GetPlacer() = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                GetPlacer() = AsStatementList(statement);
            }
        }
        public abstract class SameBehaviourCodeGenericInfo<T> : CodeGenericInfo<T> where T : BaseStatement
        {
            public SameBehaviourCodeGenericInfo(T concrete, int position) : base(concrete, position) { }

            public abstract ref BaseExpression GetPlacer();

            public override void MakeValue(object any)
            {
                GetPlacer() = new StaticExpression(any);
            }
            
            public override void MakeType(TypenameExpression type)
            {
                GetPlacer() = type;
            }

            public override void MakeName(NameExpression name)
            {
                GetPlacer() = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                GetPlacer() = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                GetPlacer() = AsStatementList(statement);
            }
        }
        #region GenericInfoDefinitions

        public class InvokeExpressionNamePartGeneric : SameBehaviourExprGenericInfo<InvokeExpression>
        {
            public InvokeExpressionNamePartGeneric(InvokeExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class InvokeExpressionArgPartGeneric : SameBehaviourExprGenericInfo<InvokeExpression>
        {
            public int ArgumentPosition;
            public InvokeExpressionArgPartGeneric(InvokeExpression concrete, int position, int argument) : base(concrete, position) {
                ArgumentPosition = argument;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition];
        }
        public class BinaryOperatorGeneric : SameBehaviourExprGenericInfo<BinaryOperator>
        {
            public bool IsRight;

            public BinaryOperatorGeneric(BinaryOperator concrete, int position, bool is_right) : base(concrete, position) {
                IsRight = is_right;
            }

            public override ref BaseExpression GetPlacer()
            {
                if (IsRight) return ref Concrete.Right;
                else return ref Concrete.Left;
            }
        }
        public class UnaryOperatorGeneric : SameBehaviourExprGenericInfo<UnaryOperator>
        {
            public UnaryOperatorGeneric(UnaryOperator concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class CastExpressionLeftPartGeneric : SameBehaviourExprGenericInfo<CastExpression>
        {
            public CastExpressionLeftPartGeneric(CastExpression concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class CastExpressionRightPartGeneric : ExprGenericInfo<CastExpression>
        {
            public CastExpressionRightPartGeneric(CastExpression concrete, int position) : base(concrete, position) { }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.Type = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Type = new TypenameExpression(name, name.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }
        }

        public class ReturnStatementGeneric : SameBehaviourCodeGenericInfo<ReturnStatement>
        {
            public ReturnStatementGeneric(ReturnStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class ExpressionStatementGeneric : SameBehaviourCodeGenericInfo<ExpressionStatement>
        {
            public ExpressionStatementGeneric(ExpressionStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        #endregion

        public class GenericMethodSignature : GenericInfo
        {
            public TemplateMethod Method;
            public int position;

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, null, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                if (position == -1) Method.ReturnType = type;
                else Method.ParamTypes[position] = type;
            }

            public override void MakeName(NameExpression name)
            {
                if (position == -1) Method.ReturnType = new TypenameExpression(name, name.SourceContext);
                else Method.ParamTypes[position] = new TypenameExpression(name, name.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                if (position == -1) Method.ReturnType = new TypenameExpression(expression, expression.SourceContext);
                else Method.ParamTypes[position] = new TypenameExpression(expression, expression.SourceContext);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, null, this);
            }
        }
        internal class GenericFinder : AbstractVisitor
        {
            public string[] Generics;
            public GenericMakingConstraint[] PredefinedConstraints;
            public List<(int, GenericMakingConstraint, GenericMakingConstraint, ExecutionContext.IExecutable)> GainedConstraints = new List<(int, GenericMakingConstraint, GenericMakingConstraint, ExecutionContext.IExecutable)>();

            private void StealConstraint(int position, GenericMakingConstraint constraint, ExecutionContext.IExecutable executable)
            {
                var nc = PredefinedConstraints[position];
                if (nc.HasFlag(constraint))
                    nc ^= constraint;
                if (nc == 0)
                    throw new ContraitConstraint(Generics[position], constraint, executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
                FixContraint(position, nc, executable);
            }
            private void FixContraint(int position, GenericMakingConstraint constraint, ExecutionContext.IExecutable executable)
            {
                if (!PredefinedConstraints[position].HasFlag(constraint))
                    throw new ContraitConstraint(Generics[position], constraint, executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
                else GainConstraint(position, constraint, executable);
            }
            private GenericMakingConstraint BinarySum(IEnumerable<GenericMakingConstraint> constraints)
            {
                var sum = (GenericMakingConstraint)0;
                foreach (var x in constraints)
                    sum |= x;
                return sum;
            }
            private void CheckAllow(int position, ExecutionContext.IExecutable executable, params GenericMakingConstraint[] contraints)
            {
                if (contraints.Any(x => !PredefinedConstraints[position].HasFlag(x)))
                    throw new ContraitConstraint(Generics[position], contraints.First(x => !PredefinedConstraints[position].HasFlag(x)), executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
                else GainConstraint(position, BinarySum(contraints), executable);
            }
            private void GainConstraint(int position, GenericMakingConstraint constraint, ExecutionContext.IExecutable executable)
            {
                for (var (i, j) = ((int)PredefinedConstraints[position], (int)constraint); j > 0; i >>= 1, j >>= 1)
                {
                    if ((i & 0b1) > (j & 0b1))
                    {
                        GainedConstraints.Add((position, PredefinedConstraints[position], constraint, executable));
                        PredefinedConstraints[position] = constraint;
                        break;
                    }
                }
            }

            public TemplateMethod Method;

            public List<GenericInfo> Infos = new List<GenericInfo>();

            public override void VisitStatement(ExpressionStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (statement is ExpressionStatement expr && expr.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new ExpressionStatementGeneric(expr, i));
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
                        Infos.Add(new ReturnStatementGeneric(expr, i));
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
                            CheckAllow(i, expression, GenericMakingConstraint.AllowTypes);
                            Infos.Add(new GenericMethodSignature()
                            {
                                Method = Method,
                                GenericPosition = i,
                                position = Method.ReturnType == expression ? -1 : Array.IndexOf(Method.ParamTypes, expression)
                            });
                            break;
                        }
                    }
                    base.VisitExpression(expression);
                }
            }

            public override void VisitExpression(MemberAccess expression)
            {
                VisitExpression(expression as BinaryOperator);
            }
            public override void VisitExpression(CastExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Left is NameExpression name1 && name1.Name == Generics[i])
                    {
                        Infos.Add(new CastExpressionLeftPartGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(BinaryOperator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Left is NameExpression name && name.Name == Generics[i]) 
                    {
                        Infos.Add(new BinaryOperatorGeneric(expression, i, false));
                    }
                    if (expression is BinaryIs && expression.Right is TypenameExpression name2 && name2.Typename.ToString() == Generics[i])
                    {
                        CheckAllow(i, expression.Right, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new BinaryOperatorGeneric(expression, i, true));
                    }
                    if (expression.Right is NameExpression name3 && name3.Name == Generics[i])
                    {
                        Infos.Add(new BinaryOperatorGeneric(expression, i, true));
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
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new InvokeExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new InvokeExpressionArgPartGeneric(expression, i, j));
                    }
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

            public static (List<GenericInfo>, GenericMakingConstraint[]) FindAll(TemplateMethod method)
            {
                var gf = new GenericFinder();
                gf.Generics = method.Generics.ConvertAll(x => x.Item1.Name);
                gf.PredefinedConstraints = method.Generics.ConvertAll(x => x.Item2.GetMakingConstraint());
                gf.Method = method;
                gf.Visit(method);
                return (gf.Infos, gf.PredefinedConstraints);
            }
        }

        [Flags]
        public enum GenericMakingConstraint
        {
            AllowNames = 0x1,
            AllowTypes = 0x2,
            AllowExpressions = 0x4,
            AllowCode = 0x8,
            AllowValues = 0x10,

            AllowAll = AllowNames | AllowTypes | AllowExpressions | AllowCode | AllowValues,
        }
        public enum GenericMaking
        {
            AsValue = 0,
            AsType = 1,
            AsName = 2,
            AsExpression = 3,
            AsCode = 4,

            Constraint = 5,
            Runtime = 6,
        }
        public static GenericMaking GetMakingBasedOnConstraint(GenericMakingConstraint constraint)
        {
            if (constraint.HasFlag(GenericMakingConstraint.AllowValues))
                return GenericMaking.AsValue;
            if (constraint.HasFlag(GenericMakingConstraint.AllowCode))
                return GenericMaking.AsCode;
            if (constraint.HasFlag(GenericMakingConstraint.AllowExpressions))
                return GenericMaking.AsExpression;
            if (constraint.HasFlag(GenericMakingConstraint.AllowNames))
                return GenericMaking.AsName;
            if (constraint.HasFlag(GenericMakingConstraint.AllowTypes))
                return GenericMaking.AsType;
            throw new ArgumentException(nameof(constraint));
        }
        public static GenericMaking GetMakingBasedOnRuntime(GenericMakingConstraint constraint, object value)
        {
            return GenericMaking.AsValue;
        }

        public TemplateMethod(string name, string[] paramNames, StatementList statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ContextWrap definitionPlace, bool @implicit, bool recursive, bool without_params, BaseExpression[] default_values, (NameExpression, Constraint)[] generics) : base(name, paramNames, statements, paramTypes, returnType, definitionPlace, @implicit, recursive, without_params, default_values)
        {
            Generics = generics;
            (GenericsInfo, MakingConstraints) = GenericFinder.FindAll(this);
            DefinitionParamTypes = ParamTypes.CloneArray();
            DefinitionReturnType = ReturnType.CloneCast(); 
        }

        public readonly TypenameExpression[] DefinitionParamTypes;
        public readonly TypenameExpression DefinitionReturnType;

        public Method MakeGenericMethod((GenericMaking, object)[] args)
        {
            for (var i = 0; i < Generics.Length; i++)
                if (!Generics[i].Item2.Applicable(args[i].Item1, args[i].Item2))
                    throw new RuntimeError($"{args[i].Item2.GetType().GetTypeString()} {args[i].Item2} doesn't fit the \"{Generics[i].Item2}\"", Generics[i].Item2.SourceContext);
            var constraints = args.Select((x, i) => x.Item1 == GenericMaking.Constraint ? GetMakingBasedOnConstraint(MakingConstraints[i]) : x.Item1).ToArray();
            foreach (var x in GenericsInfo)
            {
                var pos = x.GenericPosition;
                var making = constraints[pos];
                if (making == GenericMaking.Runtime)
                    making = GetMakingBasedOnRuntime(MakingConstraints[pos], args[pos].Item2);
                x.Make(making, args[pos].Item2);
            }

            return base.CloneWithNewName(Name);
        }

        public readonly (NameExpression, Constraint)[] Generics;
        public readonly GenericMakingConstraint[] MakingConstraints;
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
            return new TemplateMethod(name, ParamNames?.CloneArray(), Statements.CloneCast(), DefinitionParamTypes.CloneArray(), DefinitionReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, DefaultValues.CloneArray(), Generics.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())))
            {
                Abstract = Abstract
            };
        }
    }
}
