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
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static SLThree.TemplateMethod.GenericInfo;

namespace SLThree
{
    public class TemplateMethod : Method
    {
        #region Constraints

        #region Definitions
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
                    else
                    {
                        if (v.Item1 is Constraint constraint)
                            return constraint;
                        else return new ConcrecteTypeConstraint((Type)this.Name.GetValue(context), SourceContext.CloneCast());
                    }
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
                var st = Statement.CloneCast();
                return new FunctionConstraint(new Method("constraint", new string[1] { current_template }, new StatementList(new BaseStatement[1] { st }, st.SourceContext), new TypenameExpression[1], null, context.wrap, true, false, true, new BaseExpression[0]), SourceContext.CloneCast());
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
        #endregion
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

        #endregion

        #region GenericInfos

        #region Head
        public abstract class GenericInfo
        {
            public virtual object Placement { get; }
            /// <summary>
            /// Индекс шаблона (напр. для &lt;T1, T2&gt; T1 - 0, T2 - 1)
            /// </summary>
            public int GenericPosition;

            public GenericInfo(int position)
            {
                GenericPosition = position;
            }

            public TypenameExpression AsType(object any)
            {
                if (any is ClassAccess ca) return new TypenameGenericPart() { Type = ca.Name };
                return new TypenameGenericPart() { Type = (Type)any };
            }
            public NameExpression AsName(object any)
            {
                if (any is NameExpression expr) return expr;
                if (any is StringLiteral str) return new NameExpression((string)str.Value, str.SourceContext);
                if (any is string str2) return new NameExpression(str2, null);
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
            public GenericInfo(T concrete, int position) : base(position)
            {
                Concrete = concrete;
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
        #endregion

        #region Generic

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
        public class ConditionExpressionGeneric : SameBehaviourExprGenericInfo<ConditionExpression>
        {
            public ConditionExpressionGeneric(ConditionExpression concrete, int position) : base(concrete, position) { }

            public override ref BaseExpression GetPlacer() => ref Concrete.Condition;
        }
        public class NameConstraintGeneric : SameBehaviourExprGenericInfo<NameConstraintDefinition>
        {
            public NameConstraintGeneric(NameConstraintDefinition concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Name;
        }
        public class CreatorArrayTypePartGeneric : ExprGenericInfo<CreatorArray>
        {
            public CreatorArrayTypePartGeneric(CreatorArray concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.ListType = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.ListType = new TypenameExpression(name, name.SourceContext);
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
        public class CreatorArrayArgPartGeneric : SameBehaviourExprGenericInfo<CreatorArray>
        {
            public int ArgumentPosition;

            public CreatorArrayArgPartGeneric(CreatorArray concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expressions[ArgumentPosition];
        }
        public class CreatorListTypePartGeneric : ExprGenericInfo<CreatorList>
        {
            public CreatorListTypePartGeneric(CreatorList concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.ListType = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.ListType = new TypenameExpression(name, name.SourceContext);
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
        public class CreatorListArgPartGeneric : SameBehaviourExprGenericInfo<CreatorList>
        {
            public int ArgumentPosition;

            public CreatorListArgPartGeneric(CreatorList concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expressions[ArgumentPosition];
        }
        public class FunctionDefinitionNamePartGeneric : SameBehaviourExprGenericInfo<FunctionDefinition>
        {
            public FunctionDefinitionNamePartGeneric(FunctionDefinition concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.FunctionName;
        }
        public class FunctionDefinitionArgumentNamePartGeneric : ExprGenericInfo<FunctionDefinition>
        {
            public int ArgumentPosition;

            public FunctionDefinitionArgumentNamePartGeneric(FunctionDefinition concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Arguments[ArgumentPosition].Name = name;
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
        public class FunctionDefinitionArgumentTypePartGeneric : ExprGenericInfo<FunctionDefinition>
        {
            public int ArgumentPosition;
            public FunctionDefinitionArgumentTypePartGeneric(FunctionDefinition concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.Arguments[ArgumentPosition].Name.TypeHint = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Arguments[ArgumentPosition].Name.TypeHint = new TypenameExpression(name, name.SourceContext);
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
        public class FunctionDefinitionArgumentDefaultValuePartGeneric : SameBehaviourExprGenericInfo<FunctionDefinition>
        {
            public int ArgumentPosition;

            public FunctionDefinitionArgumentDefaultValuePartGeneric(FunctionDefinition concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition].DefaultValue;
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

        public class OwnParameterNameGeneric : GenericInfo
        {
            public TemplateMethod Owner;
            public int ArgumentPosition;

            public OwnParameterNameGeneric(int position, TemplateMethod owner, int argumentPosition) : base(position)
            {
                Owner = owner;
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericParameterName(GenericMaking.AsValue, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericParameterName(GenericMaking.AsType, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeName(NameExpression name)
            {
                Owner.ParamNames[ArgumentPosition] = name.Name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericParameterName(GenericMaking.AsExpression, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericParameterName(GenericMaking.AsCode, Owner.OriginalParamNames[ArgumentPosition]);
            }
        }
        public class OwnParameterTypeGeneric : GenericInfo
        {
            public TemplateMethod Owner;
            public int ArgumentPosition;

            public OwnParameterTypeGeneric(int position, TemplateMethod owner, int argumentPosition) : base(position)
            {
                Owner = owner;
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsValue, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeType(TypenameExpression type)
            {
                Owner.ParamTypes[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Owner.ParamTypes[ArgumentPosition] = new TypenameExpression(name, name.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsExpression, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsCode, Owner.OriginalParamNames[ArgumentPosition]);
            }
        }
        public class OwnReturnTypeGeneric : GenericInfo
        {
            public TemplateMethod Owner;

            public OwnReturnTypeGeneric(int position, TemplateMethod owner) : base(position)
            {
                Owner = owner;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsValue);
            }

            public override void MakeType(TypenameExpression type)
            {
                Owner.ReturnType = type;
            }

            public override void MakeName(NameExpression name)
            {
                Owner.ReturnType = new TypenameExpression(name, name.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsExpression);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsCode);
            }
        }
        public class OwnDefaultValueGeneric : GenericInfo
        {
            public TemplateMethod Owner;
            public int ArgumentPosition;

            public OwnDefaultValueGeneric(int position, TemplateMethod owner, int argumentPosition) : base(position)
            {
                Owner = owner;
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                Owner.DefaultValues[ArgumentPosition] = new StaticExpression(any);
            }

            public override void MakeType(TypenameExpression type)
            {
                Owner.DefaultValues[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Owner.DefaultValues[ArgumentPosition] = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Owner.DefaultValues[ArgumentPosition] = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                Owner.DefaultValues[ArgumentPosition] = AsStatementList(statement);
            }
        }
        #endregion

        #endregion

        public class GenericFinder : AbstractVisitor
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
            private void CheckAllAllow(int position, ExecutionContext.IExecutable executable, params GenericMakingConstraint[] contraints)
            {
                if (contraints.Any(x => !PredefinedConstraints[position].HasFlag(x)))
                    throw new ContraitConstraint(Generics[position], contraints.First(x => !PredefinedConstraints[position].HasFlag(x)), executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
                else GainConstraint(position, contraints.Aggregate((x, y) => x | y), executable);
            }
            private void CheckAnyAllow(int position, ExecutionContext.IExecutable executable, params GenericMakingConstraint[] contraints)
            {
                if (!contraints.Any(x => PredefinedConstraints[position].HasFlag(x)))
                    throw new ContraitConstraint(Generics[position], contraints.Aggregate((x, y) => x | y), executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
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
                    if ((!expression.as_is) && (!expression.Type.is_array) && expression.Type.Typename is NameExpression name3 && name3.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Type, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                        Infos.Add(new CastExpressionRightPartGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorArray expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if ((!(expression.ListType?.is_array ?? false)) && expression.ListType?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.ListType, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                        Infos.Add(new CreatorArrayTypePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Expressions.Length; j++)
                    {
                        if (expression.Expressions[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new CreatorArrayArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorList expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if ((!(expression.ListType?.is_array ?? false)) && expression.ListType?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.ListType, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                        Infos.Add(new CreatorListTypePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Expressions.Length; j++)
                    {
                        if (expression.Expressions[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new CreatorListArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(ConditionExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Condition is NameExpression name1 && name1.Name == Generics[i])
                    {
                        Infos.Add(new ConditionExpressionGeneric(expression, i));
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
                        CheckAllAllow(i, expression.Right, GenericMakingConstraint.AllowTypes);
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

            public override void VisitConstraint(NameConstraintDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.Name is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new NameConstraintGeneric(expression, i));
                }
                base.VisitConstraint(expression);
            }

            public override void VisitExpression(FunctionDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (expression.FunctionName is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new FunctionDefinitionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j].Name is NameExpression arg_name && arg_name.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowValues);
                            Infos.Add(new FunctionDefinitionArgumentNamePartGeneric(expression, i, j));
                        }
                        if (expression.Arguments[j].Name.TypeHint?.Typename is NameExpression arg_name3 && !expression.Arguments[j].Name.TypeHint.is_array && arg_name3.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowValues);
                            Infos.Add(new FunctionDefinitionArgumentNamePartGeneric(expression, i, j));
                        }
                        if (expression.Arguments[j].DefaultValue is NameExpression arg_name2 && arg_name2.Name == Generics[i])
                        {
                            Infos.Add(new FunctionDefinitionArgumentDefaultValuePartGeneric(expression, i, j));
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void Visit(Method method)
            {
                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                for (var i = 0; i < method.DefaultValues.Length; i++)
                    if (method.DefaultValues[i] != null)
                        VisitExpression(method.DefaultValues[i]);
                base.Visit(method);
            }

            public void VisitOwn(TemplateMethod method)
            {
                if (Method != method) return;

                for (var i = 0; i < Generics.Length; i++)
                {
                    for (var j = 0; j < method.ParamNames.Length; j++)
                        if (method.ParamNames[j] == Generics[i])
                        {
                            CheckAllAllow(j, method.ParamTypes[j], GenericMakingConstraint.AllowNames);
                            Infos.Add(new OwnParameterNameGeneric(i, method, j));
                        }
                    for (var j = 0; j < method.ParamTypes.Length; j++)
                        if (method.ParamTypes[j] != null)
                        {
                            if (method.ParamTypes[j].Typename is NameExpression paramTypeName && paramTypeName.Name == Generics[i])
                            {
                                CheckAnyAllow(j, method.ParamTypes[j], GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                                Infos.Add(new OwnParameterTypeGeneric(i, method, j));
                            }
                        }
                    if (method.ReturnType != null)
                        if (method.ReturnType.Typename is NameExpression retTypeName && retTypeName.Name == Generics[i])
                        {
                            CheckAnyAllow(i, method.ReturnType, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                            Infos.Add(new OwnReturnTypeGeneric(i, method));
                        }
                    for (var j = 0; j < method.DefaultValues.Length; j++)
                        if (method.DefaultValues[j] != null)
                        {
                            if (method.DefaultValues[j] is NameExpression defaultValue && defaultValue.Name == Generics[i])
                                Infos.Add(new OwnDefaultValueGeneric(i, method, j));
                        }
                }


                for (var i = 0; i < method.ParamTypes.Length; i++)
                    if (method.ParamTypes[i] != null)
                        VisitExpression(method.ParamTypes[i]);
                if (method.ReturnType != null)
                    VisitExpression(method.ReturnType);
                for (var i = 0; i < method.DefaultValues.Length; i++)
                    if (method.DefaultValues[i] != null)
                        VisitExpression(method.DefaultValues[i]);
                for (var i = 0; i < method.Statements.Statements.Length; i++)
                    VisitStatement(method.Statements.Statements[i]);
            }

            public static (List<GenericInfo>, GenericMakingConstraint[]) FindAll(TemplateMethod method)
            {
                var gf = new GenericFinder();
                gf.Generics = method.Generics.ConvertAll(x => x.Item1.Name);
                gf.PredefinedConstraints = method.Generics.ConvertAll(x => x.Item2.GetMakingConstraint());
                gf.Method = method;
                gf.VisitOwn(method);
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
            OriginalParamNames = ParamNames.CloneArray();
            OriginalParamTypes = ParamTypes.CloneArray();
            OriginalReturnType = ReturnType.CloneCast();
            OriginalDefaultValues = DefaultValues.CloneCast();
            OriginalStatements = Statements.CloneCast();
            (GenericsInfo, MakingConstraints) = GenericFinder.FindAll(this);
        }

        public readonly string[] OriginalParamNames;
        public readonly TypenameExpression[] OriginalParamTypes;
        public readonly TypenameExpression OriginalReturnType;
        public readonly BaseExpression[] OriginalDefaultValues;
        public readonly StatementList OriginalStatements;

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

            return CreateMethod();
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
            sb.Append($"({OriginalParamTypes.ConvertAll(x => x?.ToString() ?? "any").JoinIntoString(", ")})");
            sb.Append($": {OriginalReturnType?.ToString() ?? "any"}");
            return sb.ToString();
        }

        public List<GenericInfo> GenericsInfo;

        public override Method CloneWithNewName(string name)
        {
            return new TemplateMethod(name, ParamNames?.CloneArray(), OriginalStatements.CloneCast(), OriginalParamTypes.CloneArray(), OriginalReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, OriginalDefaultValues.CloneArray(), Generics.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())))
            {
                Abstract = Abstract
            };
        }
        public Method CreateMethod()
        {
            return new Method(Name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, DefaultValues.CloneArray())
            {
                Abstract = Abstract
            };
        }
    }
}
