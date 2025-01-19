using SLThree.Extensions;
using SLThree.Extensions.Cloning;

using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SLThree
{
    public sealed class TemplateMethod : Method
    {
        #region Constraints
        
        #region Definitions
        public abstract class ConstraintDefinition : BaseExpression
        {
            public ConstraintDefinition(ISourceContext context) : base(context) { }
            public ConstraintDefinition(bool priority, ISourceContext context) : base(context) { }

            public override object GetValue(ExecutionContext context) => throw new NotSupportedException();
            public abstract Constraint GetConstraint(string current_template, ExecutionContext context);
        }
        public class NameConstraintDefinition : ConstraintDefinition
        {
            public BaseExpression Name;

            public NameConstraintDefinition(BaseExpression name, ISourceContext context) : base(context)
            {
                Name = name;
            }

            public NameConstraintDefinition(BaseExpression name, bool priority, ISourceContext context) : base(context)
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
                throw new ConstraintNotFound(Name, SourceContext);
            }
            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                if (Name is TypenameExpression te && (te.Generics?.Length ?? 0) == 0 && !te.is_array)
                    return GetNameConstraint(te.Typename, current_template, context);
                else return new ConcrecteTypeConstraint((Type)Name.GetValue(context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => Name.ToString();
            public override object Clone() => new NameConstraintDefinition(Name.CloneCast(), SourceContext.CloneCast());
        }
        public class FunctionConstraintDefinition : ConstraintDefinition
        {
            public BaseStatement Statement;
            public FunctionConstraintDefinition(BaseStatement statement, ISourceContext context) : base(context)
            {
                Statement = statement;
            }

            public FunctionConstraintDefinition(BaseStatement statement, bool priority, ISourceContext context) : base(context)
            {
                Statement = statement;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                var st = Statement.CloneCast();
                return new FunctionConstraint(new Method("constraint", new string[1] { current_template }, new StatementList(new BaseStatement[1] { st }, st.SourceContext), new TypenameExpression[1], null, context.wrap, true, false, true, new BaseExpression[0], new bool[1]), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"=> {Statement}";

            public override object Clone() => new FunctionConstraintDefinition(Statement.CloneCast(), SourceContext.CloneCast());
        }
        public class CombineConstraintDefinition : ConstraintDefinition
        {
            public ConstraintDefinition Left, Right;

            public CombineConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public CombineConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, bool priority, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new CombineConstraint(Left.GetConstraint(current_template, context), Right.GetConstraint(current_template, context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"{Left} + {Right}";

            public override object Clone() => new CombineConstraintDefinition(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
        public class IntersectionConstraintDefinition : ConstraintDefinition
        {
            public ConstraintDefinition Left, Right;

            public IntersectionConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public IntersectionConstraintDefinition(ConstraintDefinition left, ConstraintDefinition right, bool priority, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new IntersectionConstraint(Left.GetConstraint(current_template, context), Right.GetConstraint(current_template, context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"{Left} | {Right}";

            public override object Clone() => new IntersectionConstraintDefinition(Left.CloneCast(), Right.CloneCast(), SourceContext.CloneCast());
        }
        public class NotConstraintDefinition : ConstraintDefinition
        {
            public ConstraintDefinition Left;

            public NotConstraintDefinition(ConstraintDefinition left, ISourceContext context) : base(context)
            {
                Left = left;
            }

            public NotConstraintDefinition(ConstraintDefinition left, bool priority, ISourceContext context) : base(context)
            {
                Left = left;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return new NotConstraint(Left.GetConstraint(current_template, context), SourceContext.CloneCast());
            }

            public override string ExpressionToString() => $"!{Left}";

            public override object Clone() => new NotConstraintDefinition(Left.CloneCast(), SourceContext.CloneCast());
        }
        internal class ObjectConstraintDefinition : ConstraintDefinition
        {
            public Constraint Value;

            public ObjectConstraintDefinition(Constraint left, ISourceContext context) : base(context)
            {
                Value = left;
            }

            public override Constraint GetConstraint(string current_template, ExecutionContext context)
            {
                return Value;
            }

            public override string ExpressionToString() => Value.ToString();

            public override object Clone() => new ObjectConstraintDefinition(Value.CloneCast(), SourceContext.CloneCast());
        }
        #endregion
        public abstract class Constraint : ICloneable
        {
            public bool PrioriryRaised { get; set; }
            public ISourceContext IISourceContext { get; set; }
            public Constraint() { }
            public Constraint(ISourceContext context) => IISourceContext = context;
            public Constraint(bool priority, ISourceContext context) => (IISourceContext, PrioriryRaised) = (context, priority);
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
            public static GenericMakingConstraint NotMakingConstraint(GenericMakingConstraint left)
            {
                const GenericMakingConstraint UndeniableConstraints = GenericMakingConstraint.AllowTypes;
                return left;
            }
            public bool ApplicateArgs(object[] args, BaseExpression makingMethod = null)
            {
                var making = GenericMaking.AsValue;
                if (makingMethod is NameExpression name)
                {
                    switch (name.Name)
                    {
                        case "code": making = GenericMaking.AsCode; break;
                        case "name": making = GenericMaking.AsName; break;
                        case "value": making = GenericMaking.AsValue; break;
                        case "constraint": making = GenericMaking.AsConstraint; break;
                        case "type": making = GenericMaking.AsType; break;
                        case "expr": making = GenericMaking.AsExpression; break;
                        default: return args.All(x => Applicable(making, x));
                    }
                    return args.Skip(1).All(x => Applicable(making, x));
                }
                return args.All(x => Applicable(making, x));
            }
            public abstract object Clone();
        }
        public class AnyConstraint : Constraint
        {
            public AnyConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"any";

            public override bool Applicable(GenericMaking making, object target) => true;

            public override object Clone() => new AnyConstraint(IISourceContext.CloneCast());

            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
        }
        public class ValueConstraint : Constraint
        {
            public ValueConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"value";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsValue);
            }

            public override object Clone() => new ValueConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowValues;
        }
        public class NameConstraint : Constraint
        {
            public NameConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"name";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsName);
            }

            public override object Clone() => new NameConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowNames;
        }
        public class TypeConstraint : Constraint
        {
            public TypeConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"type";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsType);
            }

            public override object Clone() => new TypeConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowTypes;
        }
        public class ConcrecteTypeConstraint : Constraint
        {
            public readonly Type Type;
            public ConcrecteTypeConstraint(Type type, ISourceContext context) : base(context) => Type = type;
            public override string ConstraintToString() => $"{Type.GetTypeString()}";

            public override bool Applicable(GenericMaking making, object target)
            {
                return target?.GetType().IsType(Type) ?? false;
            }

            public override object Clone() => new ConcrecteTypeConstraint(Type, IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
        }
        public class ExprConstraint : Constraint
        {
            public ExprConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"expr";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsExpression);
            }

            public override object Clone() => new ExprConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowExpressions;
        }
        public class CodeConstraint : Constraint
        {
            public CodeConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"code";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsCode);
            }

            public override object Clone() => new CodeConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowCode;
        }
        public class ConstraintConstraint : Constraint
        {
            public ConstraintConstraint(ISourceContext context) : base(context) { }
            public override string ConstraintToString() => $"constraint";

            public override bool Applicable(GenericMaking making, object target)
            {
                return making.HasFlag(GenericMaking.AsConstraint);
            }

            public override object Clone() => new ConstraintConstraint(IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowConstraints;
        }
        public class CombineConstraint : Constraint
        {
            public Constraint Left;
            public Constraint Right;

            public CombineConstraint(Constraint left, Constraint right, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }

            public override bool Applicable(GenericMaking making, object target)
            {
                return Left.Applicable(making, target) && Right.Applicable(making, target);
            }

            public override string ConstraintToString() => $"{Left} + {Right}";

            public override object Clone() => new CombineConstraint(Left.CloneCast(), Right.CloneCast(), IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => CombineMakingConstraint(Left.GetMakingConstraint(), Right.GetMakingConstraint());
        }
        public class IntersectionConstraint : Constraint
        {
            public Constraint Left;
            public Constraint Right;

            public IntersectionConstraint(Constraint left, Constraint right, ISourceContext context) : base(context)
            {
                Left = left;
                Right = right;
            }
            public override bool Applicable(GenericMaking making, object target)
            {
                return Left.Applicable(making, target) || Right.Applicable(making, target);
            }
            public override string ConstraintToString() => $"{Left} | {Right}";
            public override object Clone() => new IntersectionConstraint(Left.CloneCast(), Right.CloneCast(), IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => IntersectionMakingConstraint(Left.GetMakingConstraint(), Right.GetMakingConstraint());
        }
        public class NotConstraint : Constraint
        {
            public Constraint Left;

            public NotConstraint(Constraint left, ISourceContext context) : base(context)
            {
                Left = left;
            }

            public override bool Applicable(GenericMaking making, object target)
            {
                return !Left.Applicable(making, target);
            }
            public override string ConstraintToString() => $"!{Left}";
            public override object Clone() => new NotConstraint(Left.CloneCast(), IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => NotMakingConstraint(Left.GetMakingConstraint());
        }
        public class FunctionConstraint : Constraint
        {
            public FunctionConstraint(Method method, ISourceContext context) : base(context)
            {
                Predicate = method;
            }
            public readonly Method Predicate;
            public override string ConstraintToString() => $"=> ...";

            public override bool Applicable(GenericMaking making, object target)
            {
                return Predicate.Invoke(target).Cast<bool>();
            }

            public override object Clone() => new FunctionConstraint(Predicate.CloneCast(), IISourceContext.CloneCast());
            public override GenericMakingConstraint GetMakingConstraint() => GenericMakingConstraint.AllowAll;
        }
        public class CustomConstraint : Constraint
        {
            public string Name;
            public readonly Constraint Constraint;

            public CustomConstraint(string name, Constraint constraint, ISourceContext context) : base(context)
            {
                Name = name;
                Constraint = constraint;
            }

            public override string ConstraintToString() => $"constraint {Name}: {Constraint}";

            public override bool Applicable(GenericMaking making, object target)
            {
                return Constraint.Applicable(making, target);
            }

            public override object Clone() => new CustomConstraint(Name, Constraint.CloneCast(), IISourceContext.CloneCast());
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
                throw new RuntimeError(string.Format(Locale.Current["ERR_NotExpression"], any?.GetType().GetTypeString() ?? "null"), ContraitConstraint.TakeContext(Placement as ExecutionContext.IExecutable));
            }
            public BaseStatement AsStatement(object any)
            {
                if (any is BaseStatement st1) return st1;
                if (any is BlockExpression be) return new StatementList(be.Statements, be.SourceContext.CloneCast());
                throw new RuntimeError(string.Format(Locale.Current["ERR_NotCode"], any?.GetType().GetTypeString() ?? "null"), ContraitConstraint.TakeContext(Placement as ExecutionContext.IExecutable));
            }
            public BaseExpression AsStatementList(BaseStatement statement)
            {
                if (statement is StatementList sl)
                    return new BlockExpression(sl.Statements, sl.SourceContext);
                return new BlockExpression(new BaseStatement[1] { statement }, statement.SourceContext);
            }
            public Constraint AsConstraint(object any)
            {
                if (any is Constraint constraint) return constraint;
                throw new RuntimeError(string.Format(Locale.Current["ERR_NotConstraint"], any?.GetType().GetTypeString() ?? "null"), ContraitConstraint.TakeContext(Placement as ExecutionContext.IExecutable));
            }

            public abstract void MakeValue(object any);
            public abstract void MakeType(TypenameExpression type);
            public abstract void MakeName(NameExpression name);
            public abstract void MakeExpression(BaseExpression expression);
            public abstract void MakeCode(BaseStatement statement);
            public abstract void MakeConstraint(Constraint constraint);
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
                    case GenericMaking.AsConstraint:
                        MakeConstraint(AsConstraint(any));
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
                GetPlacer() = new ObjectLiteral(any, Concrete.SourceContext);
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

            public override void MakeConstraint(Constraint constraint)
            {
                GetPlacer() = new ObjectLiteral(constraint);
            }
        }
        public abstract class SameBehaviourCodeGenericInfo<T> : CodeGenericInfo<T> where T : BaseStatement
        {
            public SameBehaviourCodeGenericInfo(T concrete, int position) : base(concrete, position) { }

            public abstract ref BaseExpression GetPlacer();

            public override void MakeValue(object any)
            {
                GetPlacer() = new ObjectLiteral(any, Concrete.SourceContext);
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

            public override void MakeConstraint(Constraint constraint)
            {
                GetPlacer() = new ObjectLiteral(constraint);
            }
        }
        #endregion

        #region Generic

        public class TypenamePullerGeneric : GenericInfo<TypenameExpression>
        {
            public TypenamePullerGeneric(TypenameExpression concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any) => Concrete.PullTheType();
            public override void MakeType(TypenameExpression type) => Concrete.PullTheType();
            public override void MakeName(NameExpression name) => Concrete.PullTheType();
            public override void MakeExpression(BaseExpression expression) => Concrete.PullTheType();
            public override void MakeCode(BaseStatement statement) => Concrete.PullTheType();
            public override void MakeConstraint(Constraint constraint) => Concrete.PullTheType();
        }
        public class TypenameGenericArgGeneric : GenericInfo<TypenameExpression>
        {
            public int ArgumentPosition;
            public TypenameGenericArgGeneric(TypenameExpression concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.Generics[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Generics[ArgumentPosition] = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class InvokeExpressionNamePartGeneric : SameBehaviourExprGenericInfo<InvokeExpression>
        {
            public InvokeExpressionNamePartGeneric(InvokeExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
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
        public class TernaryOperatorConditionPartGeneric : SameBehaviourExprGenericInfo<TernaryOperator>
        {
            public TernaryOperatorConditionPartGeneric(TernaryOperator concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Condition;
        }
        public class TernaryOperatorBodyPartGeneric : SameBehaviourExprGenericInfo<TernaryOperator>
        {
            public bool IsRight;

            public TernaryOperatorBodyPartGeneric(TernaryOperator concrete, int position, bool is_right) : base(concrete, position)
            {
                IsRight = is_right;
            }

            public override ref BaseExpression GetPlacer()
            {
                if (IsRight) return ref Concrete.Right;
                else return ref Concrete.Left;
            }
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
                Concrete.Type = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
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
        public class FunctionDefinitionNamePartGeneric : ExprGenericInfo<FunctionDefinition>
        {
            public FunctionDefinitionNamePartGeneric(FunctionDefinition concrete, int position) : base(concrete, position) { }

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
                Concrete.FunctionName = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.FunctionName = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
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

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
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
                Concrete.Arguments[ArgumentPosition].Name.TypeHint = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
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
        public class FunctionDefinitionGenericArgumentConstraintPartGeneric : ExprGenericInfo<FunctionDefinition>
        {
            public int ArgumentPosition;
            public FunctionDefinitionGenericArgumentConstraintPartGeneric(FunctionDefinition concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new NameConstraintDefinition(type, Concrete.SourceContext);
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new NameConstraintDefinition(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectConstraintDefinition(constraint, Concrete.SourceContext);
            }
        }
        public class FunctionDefinitionReturnTypePartGeneric : ExprGenericInfo<FunctionDefinition>
        {
            public FunctionDefinitionReturnTypePartGeneric(FunctionDefinition concrete, int position) : base(concrete, position)
            {

            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.ReturnTypeHint = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.ReturnTypeHint = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class BaseInstanceCreatorNamePartGeneric : ExprGenericInfo<BaseInstanceCreator>
        {
            public BaseInstanceCreatorNamePartGeneric(BaseInstanceCreator concrete, int position) : base(concrete, position)
            {
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
                Concrete.Name = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.Name = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class BaseInstanceCreatorTypePartGeneric : ExprGenericInfo<BaseInstanceCreator>
        {
            public BaseInstanceCreatorTypePartGeneric(BaseInstanceCreator concrete, int position) : base(concrete, position)
            {
            }

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
                Concrete.Type = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class BaseInstanceCreatorArgPartGeneric : SameBehaviourExprGenericInfo<BaseInstanceCreator>
        {
            public int ArgumentPosition;
            public BaseInstanceCreatorArgPartGeneric(BaseInstanceCreator concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition];
        }
        public class CreatorContextNamePartGeneric : ExprGenericInfo<CreatorContext>
        {
            public CreatorContextNamePartGeneric(CreatorContext concrete, int position) : base(concrete, position)
            {

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
                Concrete.Name = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.Name = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class CreatorContextAncestorPartGeneric : SameBehaviourExprGenericInfo<CreatorContext>
        {
            public int ArgumentPosition;
            public CreatorContextAncestorPartGeneric(CreatorContext concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Ancestors[ArgumentPosition];
        }
        public class CreatorNewArrayTypePartGeneric : ExprGenericInfo<CreatorNewArray>
        {
            public CreatorNewArrayTypePartGeneric(CreatorNewArray concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.ArrayType = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.ArrayType = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class CreatorNewArraySizePartGeneric : SameBehaviourExprGenericInfo<CreatorNewArray>
        {
            public CreatorNewArraySizePartGeneric(CreatorNewArray concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Size;
        }
        public class CreatorRangeTypePartGeneric : ExprGenericInfo<CreatorRange>
        {
            public CreatorRangeTypePartGeneric(CreatorRange concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.RangeType = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.RangeType = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class CreatorRangeBoundPartGeneric : SameBehaviourExprGenericInfo<CreatorRange>
        {
            public bool IsUpper;
            public CreatorRangeBoundPartGeneric(CreatorRange concrete, int position, bool isUpper) : base(concrete, position)
            {
                IsUpper = isUpper;
            }

            public override ref BaseExpression GetPlacer()
            {
                if (IsUpper) return ref Concrete.UpperBound;
                return ref Concrete.LowerBound;
            }
        }
        public class CreatorCollectionElementPartGeneric : SameBehaviourExprGenericInfo<CreatorCollection>
        {
            public int ArgumentPosition;
            public CreatorCollectionElementPartGeneric(CreatorCollection concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Body[ArgumentPosition];
        }
        public class CreatorDictionaryEntryPartGeneric : SameBehaviourExprGenericInfo<CreatorDictionary.DictionaryEntry>
        {
            public bool IsValue;
            public CreatorDictionaryEntryPartGeneric(CreatorDictionary.DictionaryEntry concrete, int position, bool isValue) : base(concrete, position)
            {
                IsValue = isValue;
            }

            public override ref BaseExpression GetPlacer()
            {
                if (IsValue) return ref Concrete.Value;
                return ref Concrete.Key;
            }
        }
        public class CreatorTupleElementPartGeneric : SameBehaviourExprGenericInfo<CreatorTuple>
        {
            public int ArgumentPosition;
            public CreatorTupleElementPartGeneric(CreatorTuple concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expressions[ArgumentPosition];
        }
        public class NameExpressionTypeGeneric : ExprGenericInfo<NameExpression>
        {
            public NameExpressionTypeGeneric(NameExpression concrete, int position) : base(concrete, position) { }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.TypeHint = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.TypeHint = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class CreatorUsingGeneric : ExprGenericInfo<CreatorUsing>
        {
            public CreatorUsingGeneric(CreatorUsing concrete, int position) : base(concrete, position)
            {
            }

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
                Concrete.Type = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class UsingExpressionAliasPartGeneric : ExprGenericInfo<UsingExpression>
        {
            public UsingExpressionAliasPartGeneric(UsingExpression concrete, int position) : base(concrete, position)
            {
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
                Concrete.Alias = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.Alias = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class StaticExpressionGeneric : SameBehaviourExprGenericInfo<StaticExpression>
        {
            public StaticExpressionGeneric(StaticExpression concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Right;
        }
        public class ReferenceExpressionGeneric : SameBehaviourExprGenericInfo<ReferenceExpression>
        {
            public ReferenceExpressionGeneric(ReferenceExpression concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class DereferenceExpressionGeneric : SameBehaviourExprGenericInfo<DereferenceExpression>
        {
            public DereferenceExpressionGeneric(DereferenceExpression concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class GenericMakingDefinitionGeneric : SameBehaviourExprGenericInfo<InvokeTemplateExpression.GenericMakingDefinition>
        {
            public GenericMakingDefinitionGeneric(InvokeTemplateExpression.GenericMakingDefinition concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class IndexExpressionLeftPartGeneric : SameBehaviourExprGenericInfo<IndexExpression>
        {
            public IndexExpressionLeftPartGeneric(IndexExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class IndexExpressionArgPartGeneric : SameBehaviourExprGenericInfo<IndexExpression>
        {
            public int ArgumentPosition;
            public IndexExpressionArgPartGeneric(IndexExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }
            
            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition];
        }
        public class MacrosDefinitionGeneric : ExprGenericInfo<MacrosDefinition>
        {
            public MacrosDefinitionGeneric(MacrosDefinition concrete, int position) : base(concrete, position)
            {
            }

            public override void MakeValue(object any)
            {
                Concrete.Executable = new ObjectLiteral(any, Concrete.SourceContext.CloneCast());
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.Executable = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Executable = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.Executable = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                Concrete.Executable = AsStatementList(statement);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class InvokeGenericExpressionNamePartGeneric : SameBehaviourExprGenericInfo<InvokeGenericExpression>
        {
            public InvokeGenericExpressionNamePartGeneric(InvokeGenericExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class InvokeGenericExpressionArgPartGeneric : SameBehaviourExprGenericInfo<InvokeGenericExpression>
        {
            public int ArgumentPosition;
            public InvokeGenericExpressionArgPartGeneric(InvokeGenericExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition];
        }
        public class InvokeGenericExpressionGenericArgPartGeneric : ExprGenericInfo<InvokeGenericExpression>
        {
            public int ArgumentPosition;
            public InvokeGenericExpressionGenericArgPartGeneric(InvokeGenericExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.GenericArguments[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.GenericArguments[ArgumentPosition] = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class MakeGenericExpressionNamePartGeneric : SameBehaviourExprGenericInfo<MakeGenericExpression>
        {
            public MakeGenericExpressionNamePartGeneric(MakeGenericExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class MakeGenericExpressionGenericArgPartGeneric : ExprGenericInfo<MakeGenericExpression>
        {
            public int ArgumentPosition;
            public MakeGenericExpressionGenericArgPartGeneric(MakeGenericExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.GenericArguments[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.GenericArguments[ArgumentPosition] = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class InvokeTemplateExpressionNamePartGeneric : SameBehaviourExprGenericInfo<InvokeTemplateExpression>
        {
            public InvokeTemplateExpressionNamePartGeneric(InvokeTemplateExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class InvokeTemplateExpressionArgPartGeneric : SameBehaviourExprGenericInfo<InvokeTemplateExpression>
        {
            public int ArgumentPosition;
            public InvokeTemplateExpressionArgPartGeneric(InvokeTemplateExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Arguments[ArgumentPosition];
        }
        public class InvokeTemplateExpressionGenericArgPartGeneric : ExprGenericInfo<InvokeTemplateExpression>
        {
            public int ArgumentPosition;
            public InvokeTemplateExpressionGenericArgPartGeneric(InvokeTemplateExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override void MakeValue(object any)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectLiteral(any);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = AsStatementList(statement);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectConstraintDefinition(constraint, Concrete.SourceContext);
            }
        }
        public class InvokeTemplateExpressionGenericMakingPartGeneric : ExprGenericInfo<InvokeTemplateExpression>
        {
            public int ArgumentPosition;
            public InvokeTemplateExpressionGenericMakingPartGeneric(InvokeTemplateExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
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
                Concrete.GenericArguments[ArgumentPosition].Item1 = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class MakeTemplateExpressionNamePartGeneric : SameBehaviourExprGenericInfo<MakeTemplateExpression>
        {
            public MakeTemplateExpressionNamePartGeneric(MakeTemplateExpression concrete, int position) : base(concrete, position) { }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class MakeTemplateExpressionGenericArgPartGeneric : ExprGenericInfo<MakeTemplateExpression>
        {
            public int ArgumentPosition;
            public MakeTemplateExpressionGenericArgPartGeneric(MakeTemplateExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
            }

            public override void MakeValue(object any)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectLiteral(any);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = AsStatementList(statement);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.GenericArguments[ArgumentPosition].Item2 = new ObjectConstraintDefinition(constraint, Concrete.SourceContext);
            }
        }
        public class MakeTemplateExpressionGenericMakingPartGeneric : ExprGenericInfo<MakeTemplateExpression>
        {
            public int ArgumentPosition;
            public MakeTemplateExpressionGenericMakingPartGeneric(MakeTemplateExpression concrete, int position, int argument) : base(concrete, position)
            {
                ArgumentPosition = argument;
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
                Concrete.GenericArguments[ArgumentPosition].Item1 = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class MatchExpressionHeadPartGeneric : SameBehaviourExprGenericInfo<MatchExpression>
        {
            public MatchExpressionHeadPartGeneric(MatchExpression concrete, int position) : base(concrete, position) { }

            public override ref BaseExpression GetPlacer() => ref Concrete.Matching;
        }
        public class MatchExpressionMatchesPartGeneric : SameBehaviourExprGenericInfo<MatchExpression>
        {
            public int casePosition1, casePosition2;
            public MatchExpressionMatchesPartGeneric(MatchExpression concrete, int position, int casePosition1, int casePosition2) : base(concrete, position)
            {
                this.casePosition1 = casePosition1;
                this.casePosition2 = casePosition2;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Matches[casePosition1][casePosition2];
        }
        public class AccordExpressionHeadPartGeneric : SameBehaviourExprGenericInfo<AccordExpression>
        {
            public int position;
            public AccordExpressionHeadPartGeneric(AccordExpression concrete, int position, int accPosition) : base(concrete, position)
            {
                this.position = accPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.HeadAccords[position];
        }
        public class AccordExpressionMatchesPartGeneric : ExprGenericInfo<AccordExpression>
        {
            public int casePosition1, casePosition2;
            public bool isConstraint = false;
            public AccordExpressionMatchesPartGeneric(AccordExpression concrete, int position, int casePosition1, int casePosition2, bool isConstraint) : base(concrete, position)
            {
                this.casePosition1 = casePosition1;
                this.casePosition2 = casePosition2;
                this.isConstraint = isConstraint;
            }

            public override void MakeValue(object any)
            {
                if (isConstraint) Concrete.Accordings[casePosition1][casePosition2].Item2 = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = new ObjectLiteral(any, Concrete.SourceContext);
            }

            public override void MakeType(TypenameExpression type)
            {
                if (isConstraint) throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = type;
            }

            public override void MakeName(NameExpression name)
            {
                if (isConstraint) Concrete.Accordings[casePosition1][casePosition2].Item2 = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                if (isConstraint) throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                if (isConstraint) throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = AsStatementList(statement);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                if (isConstraint) Concrete.Accordings[casePosition1][casePosition2].Item2 = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
                else Concrete.Accordings[casePosition1][casePosition2].Item1 = new ObjectLiteral(constraint);
            }
        }
        public class ReflectionExpressionLeftPartGeneric : SameBehaviourExprGenericInfo<ReflectionExpression>
        {
            public ReflectionExpressionLeftPartGeneric(ReflectionExpression concrete, int position) : base(concrete, position) { }

            public override ref BaseExpression GetPlacer() => ref Concrete.Left;
        }
        public class ReflectionExpressionNamePartGeneric : ExprGenericInfo<ReflectionExpression>
        {
            public ReflectionExpressionNamePartGeneric(ReflectionExpression concrete, int position) : base(concrete, position)
            {

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
                Concrete.Right = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class ReflectionExpressionArgumentPartGeneric : ExprGenericInfo<ReflectionExpression>
        {
            public int ArgumentPosition;
            public ReflectionExpressionArgumentPartGeneric(ReflectionExpression concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.MethodArguments[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.MethodArguments[ArgumentPosition] = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class ReflectionExpressionGenericArgumentPartGeneric : ExprGenericInfo<ReflectionExpression>
        {
            public int ArgumentPosition;
            public ReflectionExpressionGenericArgumentPartGeneric(ReflectionExpression concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.MethodGenericArguments[ArgumentPosition] = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.MethodGenericArguments[ArgumentPosition] = new TypenameExpression(name, Concrete.SourceContext);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class InterpolatedStringGeneric : SameBehaviourExprGenericInfo<InterpolatedString>
        {
            public int ArgumentPosition;
            public InterpolatedStringGeneric(InterpolatedString concrete, int position, int argumentPosition) : base(concrete, position)
            {
                ArgumentPosition = argumentPosition;
            }

            public override ref BaseExpression GetPlacer() => ref Concrete.Expressions[ArgumentPosition];
        }
        public class ConstraintExpressionNamePartGeneric : ExprGenericInfo<ConstraintExpression>
        {
            public ConstraintExpressionNamePartGeneric(ConstraintExpression concrete, int position) : base(concrete, position)
            {

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
                Concrete.Name = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                Concrete.Name = expression;
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class ConstraintExpressionTargetPartGeneric : ExprGenericInfo<ConstraintExpression>
        {
            public ConstraintExpressionTargetPartGeneric(ConstraintExpression concrete, int position) : base(concrete, position)
            {

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
                Concrete.Target = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsConstraint, Concrete, this);
            }
        }
        public class ConstraintExpressionBodyPartGeneric : ExprGenericInfo<ConstraintExpression>
        {
            public ConstraintExpressionBodyPartGeneric(ConstraintExpression concrete, int position) : base(concrete, position)
            {

            }

            public override void MakeValue(object any)
            {
                Concrete.Body = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Body = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.Body = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
            }
        }
        public class SliceExpressionLeftPartGeneric : SameBehaviourExprGenericInfo<SliceExpression>
        {
            public SliceExpressionLeftPartGeneric(SliceExpression concrete, int position) : base(concrete, position)
            {
            }

            public override ref BaseExpression GetPlacer()
            {
                return ref Concrete.Left;
            }
        }
        public class SliceExpressionBoundPartGeneric : SameBehaviourExprGenericInfo<SliceExpression>
        {
            public bool IsUpper;
            public SliceExpressionBoundPartGeneric(SliceExpression concrete, int position, bool isUpper) : base(concrete, position)
            {
                IsUpper = isUpper;
            }

            public override ref BaseExpression GetPlacer()
            {
                if (IsUpper) return ref Concrete.UpperBound;
                return ref Concrete.LowerBound;
            }
        }

        public class ExpressionStatementGeneric : SameBehaviourCodeGenericInfo<ExpressionStatement>
        {
            public ExpressionStatementGeneric(ExpressionStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class ReturnStatementGeneric : SameBehaviourCodeGenericInfo<ReturnStatement>
        {
            public ReturnStatementGeneric(ReturnStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Expression;
        }
        public class WhileLoopStatementGeneric : SameBehaviourCodeGenericInfo<WhileLoopStatement>
        {
            public WhileLoopStatementGeneric(WhileLoopStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Condition;
        }
        public class DoWhileLoopStatementGeneric : SameBehaviourCodeGenericInfo<DoWhileLoopStatement>
        {
            public DoWhileLoopStatementGeneric(DoWhileLoopStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Condition;
        }
        public class FiniteLoopStatementGeneric : SameBehaviourCodeGenericInfo<FiniteLoopStatement>
        {
            public FiniteLoopStatementGeneric(FiniteLoopStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.Iterations;
        }
        public class ForeachLoopStatementGeneric : SameBehaviourCodeGenericInfo<ForeachLoopStatement>
        {
            public bool IsEnumerable;
            public ForeachLoopStatementGeneric(ForeachLoopStatement concrete, int position, bool is_enumerable) : base(concrete, position)
            {
                IsEnumerable = is_enumerable;
            }
            public override ref BaseExpression GetPlacer()
            {
                if (IsEnumerable) return ref Concrete.Iterator;
                return ref Concrete.Left;
            }
        }
        public class ThrowStatementGeneric : SameBehaviourCodeGenericInfo<ThrowStatement>
        {
            public ThrowStatementGeneric(ThrowStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.ThrowExpression;
        }
        public class TryStatementGeneric : SameBehaviourCodeGenericInfo<TryStatement>
        {
            public TryStatementGeneric(TryStatement concrete, int position) : base(concrete, position) { }
            public override ref BaseExpression GetPlacer() => ref Concrete.CatchVariable;
        }

        public class NameConstraintDefinitionGeneric : ExprGenericInfo<NameConstraintDefinition>
        {
            public NameConstraintDefinitionGeneric(NameConstraintDefinition concrete, int position) : base(concrete, position)
            {

            }

            public override void MakeValue(object any)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsValue, Concrete, this);
            }

            public override void MakeType(TypenameExpression type)
            {
                Concrete.Name = type;
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Name = name;
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.Name = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
            }
        }
        public class CombineConstraintDefinitionGeneric : ExprGenericInfo<CombineConstraintDefinition>
        {
            public bool IsRight;
            public CombineConstraintDefinitionGeneric(CombineConstraintDefinition concrete, int position, bool is_right) : base(concrete, position)
            {
                IsRight = is_right;
            }

            public override void MakeValue(object any)
            {
                if (IsRight) Concrete.Right = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
                Concrete.Left = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeName(NameExpression name)
            {
                if (IsRight) Concrete.Left = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
                Concrete.Left = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                if (IsRight) Concrete.Right = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
                Concrete.Left = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
            }
        }
        public class IntersectionConstraintDefinitionGeneric : ExprGenericInfo<IntersectionConstraintDefinition>
        {
            public bool IsRight;
            public IntersectionConstraintDefinitionGeneric(IntersectionConstraintDefinition concrete, int position, bool is_right) : base(concrete, position)
            {
                IsRight = is_right;
            }

            public override void MakeValue(object any)
            {
                if (IsRight) Concrete.Right = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
                Concrete.Left = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeName(NameExpression name)
            {
                if (IsRight) Concrete.Left = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
                Concrete.Left = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                if (IsRight) Concrete.Right = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
                Concrete.Left = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
            }
        }
        public class NotConstraintDefinitionGeneric : ExprGenericInfo<NotConstraintDefinition>
        {
            public NotConstraintDefinitionGeneric(NotConstraintDefinition concrete, int position) : base(concrete, position)
            {

            }

            public override void MakeValue(object any)
            {
                Concrete.Left = new ObjectConstraintDefinition(AsConstraint(any), Concrete.SourceContext.CloneCast());
            }

            public override void MakeType(TypenameExpression type)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsType, Concrete, this);
            }

            public override void MakeName(NameExpression name)
            {
                Concrete.Left = new NameConstraintDefinition(name, Concrete.SourceContext.CloneCast());
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsExpression, Concrete, this);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericMaking(GenericMaking.AsCode, Concrete, this);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                Concrete.Left = new ObjectConstraintDefinition(constraint, Concrete.SourceContext.CloneCast());
            }
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

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericParameterName(GenericMaking.AsConstraint, Owner.OriginalParamNames[ArgumentPosition]);
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
                Owner.ParamTypes[ArgumentPosition] = new TypenameExpression(name, null);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsExpression, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsCode, Owner.OriginalParamNames[ArgumentPosition]);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericParameterType(GenericMaking.AsConstraint, Owner.OriginalParamNames[ArgumentPosition]);
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
                Owner.ReturnType = new TypenameExpression(name, null);
            }

            public override void MakeExpression(BaseExpression expression)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsExpression);
            }

            public override void MakeCode(BaseStatement statement)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsCode);
            }

            public override void MakeConstraint(Constraint constraint)
            {
                throw new UnavailableGenericResultType(GenericMaking.AsConstraint);
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
                Owner.DefaultValues[ArgumentPosition] = new ObjectLiteral(any);
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

            public override void MakeConstraint(Constraint constraint)
            {
                Owner.DefaultValues[ArgumentPosition] = new ObjectLiteral(constraint);
            }
        }
        #endregion

        #endregion

        [VisitorNotice("VisitStatement", typeof(BreakStatement))]
        [VisitorNotice("VisitStatement", typeof(ContinueStatement))]
        [VisitorNotice("VisitStatement", typeof(CreatorContextBody))]
        [VisitorNotice("VisitStatement", typeof(BaseLoopStatement))]
        [VisitorNotice("VisitStatement", typeof(InfiniteLoopStatement))]
        [VisitorNotice("VisitStatement", typeof(StatementList))]
        [VisitorNotice("VisitExpression", typeof(BlockExpression))]
        [VisitorNotice("VisitExpression", typeof(CreatorInstance))]
        [VisitorNotice("VisitExpression", typeof(CreatorDictionary))]
        [VisitorNotice("VisitExpression", typeof(FunctionArgument))]
        [VisitorNotice("VisitExpression", typeof(Literal))]
        [VisitorNotice("VisitExpression", typeof(Special))]
        [VisitorNotice("VisitConstraint", typeof(FunctionConstraintDefinition))]
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
                GainConstraint(position, contraints.Aggregate((x, y) => x | y), executable);
            }
            private void CheckAnyAllow(int position, ExecutionContext.IExecutable executable, params GenericMakingConstraint[] contraints)
            {
                if (!contraints.Any(x => PredefinedConstraints[position].HasFlag(x)))
                    throw new ContraitConstraint(Generics[position], contraints.Aggregate((x, y) => x | y), executable, GainedConstraints.Where(x => x.Item1 == position).Select(x => (x.Item2, x.Item3, x.Item4)));
                GainConstraint(position, contraints.Aggregate((x, y) => x | y) & PredefinedConstraints[position], executable);
            }
            private void GainConstraint(int position, GenericMakingConstraint constraint, ExecutionContext.IExecutable executable)
            {
                for (var (i, j) = ((int)PredefinedConstraints[position], (int)constraint); j > 0 || i > 0; i >>= 1, j >>= 1)
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

            public Stack<int[]> Shadowing = new Stack<int[]>(new int[][] { new int[0] });
            public List<GenericInfo> Infos = new List<GenericInfo>();

            public bool IsShaded(int index)
            {
                return Shadowing.Peek().Contains(index);
            }

            public override void VisitConstraint(NameConstraintDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Name is TypenameExpression tp && tp.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new NameConstraintDefinitionGeneric(expression, i));
                    }
                    else if (expression.Name is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new NameConstraintDefinitionGeneric(expression, i));
                    }
                }
                base.VisitConstraint(expression);
            }

            public override void VisitConstraint(NotConstraintDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameConstraintDefinition name1 && name1.Name is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowConstraints);
                        Infos.Add(new NotConstraintDefinitionGeneric(expression, i));
                    }
                }
                base.VisitConstraint(expression);
            }

            public override void VisitConstraint(IntersectionConstraintDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameConstraintDefinition name1 && name1.Name is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowConstraints);
                        Infos.Add(new IntersectionConstraintDefinitionGeneric(expression, i, false));
                    }
                    if (expression.Right is NameConstraintDefinition name3 && name3.Name is NameExpression name4 && name4.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowConstraints);
                        Infos.Add(new IntersectionConstraintDefinitionGeneric(expression, i, true));
                    }
                }
                base.VisitConstraint(expression);
            }

            public override void VisitConstraint(CombineConstraintDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameConstraintDefinition name1 && name1.Name is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowConstraints);
                        Infos.Add(new CombineConstraintDefinitionGeneric(expression, i, false));
                    }
                    if (expression.Right is NameConstraintDefinition name3 && name3.Name is NameExpression name4 && name4.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowConstraints);
                        Infos.Add(new CombineConstraintDefinitionGeneric(expression, i, true));
                    }
                }
                base.VisitConstraint(expression);
            }

            public override void VisitExpression(NameExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.TypeHint?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new NameExpressionTypeGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(TypenameExpression expression)
            {
                var count = Infos.Count;
                base.VisitExpression(expression.Typename);
                if (Infos.Count > count)
                    Infos.Add(new TypenamePullerGeneric(expression, 0));

                var onbasevisit = new HashSet<TypenameExpression>();

                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Generics != null)
                    {
                        Executables.Add(expression);
                        for (var j = 0; j < expression.Generics.Length; j++)
                        {
                            count = Infos.Count;
                            if (expression.Generics[j].Typename is NameExpression name && name.Name == Generics[i])
                            {
                                CheckAnyAllow(i, expression.Generics[j], GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                                Infos.Add(new TypenameGenericArgGeneric(expression, i, j));
                            }
                            else onbasevisit.Add(expression.Generics[j]);
                            if (Infos.Count > count)
                                Infos.Add(new TypenamePullerGeneric(expression.Generics[j], 0));
                        }
                        Executables.Remove(expression);
                    }
                }

                foreach (var x in onbasevisit)
                    base.VisitExpression(x);
            }

            public override void VisitExpression(MemberAccess expression)
            {
                VisitExpression(expression as BinaryOperator);
            }

            public override void VisitExpression(CastExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
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

            public override void VisitExpression(ConditionExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Condition is NameExpression name1 && name1.Name == Generics[i])
                    {
                        Infos.Add(new ConditionExpressionGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(TernaryOperator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Condition is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new TernaryOperatorConditionPartGeneric(expression, i));
                    }
                    if (expression.Left is NameExpression name2 && name2.Name == Generics[i])
                    {
                        Infos.Add(new TernaryOperatorBodyPartGeneric(expression, i, false));
                    }
                    if (expression.Right is NameExpression name3 && name3.Name == Generics[i])
                    {
                        Infos.Add(new TernaryOperatorBodyPartGeneric(expression, i, true));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(BinaryOperator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i]) 
                    {
                        Infos.Add(new BinaryOperatorGeneric(expression, i, false));
                    }
                    if (expression is BinaryIs && expression.Right is TypenameExpression name2 && name2.Typename.ToString() == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Right, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
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
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new UnaryOperatorGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(IndexExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new IndexExpressionLeftPartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new IndexExpressionArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(InvokeExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
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

            public override void VisitExpression(InvokeGenericExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new InvokeGenericExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.GenericArguments.Length; j++)
                    {
                        if (expression.GenericArguments[j]?.Typename is NameExpression name1 && name1.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.GenericArguments[j], GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                            Infos.Add(new InvokeGenericExpressionGenericArgPartGeneric(expression, i, j));
                        }
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new InvokeGenericExpressionArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(MakeGenericExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new MakeGenericExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.GenericArguments.Length; j++)
                    {
                        if (expression.GenericArguments[j]?.Typename is NameExpression name1 && name1.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.GenericArguments[j], GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                            Infos.Add(new MakeGenericExpressionGenericArgPartGeneric(expression, i, j));
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(InvokeTemplateExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new InvokeTemplateExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.GenericArguments.Length; j++)
                    {
                        if (expression.GenericArguments[j].Item1 is NameExpression name1 && name1.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.GenericArguments[j].Item1, GenericMakingConstraint.AllowNames);
                            Infos.Add(new InvokeTemplateExpressionGenericMakingPartGeneric(expression, i, j));
                        }
                        if (expression.GenericArguments[j].Item2 is NameExpression name2 && name2.Name == Generics[i])
                        {
                            Infos.Add(new InvokeTemplateExpressionGenericArgPartGeneric(expression, i, j));
                        }
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j] is NameExpression arg_name && arg_name.Name == Generics[i])
                            Infos.Add(new InvokeTemplateExpressionArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(MakeTemplateExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new MakeTemplateExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.GenericArguments.Length; j++)
                    {
                        if (expression.GenericArguments[j].Item1 is NameExpression name1 && name1.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.GenericArguments[j].Item2, GenericMakingConstraint.AllowNames);
                            Infos.Add(new MakeTemplateExpressionGenericMakingPartGeneric(expression, i, j));
                        }
                        if (expression.GenericArguments[j].Item2 is NameExpression name2 && name2.Name == Generics[i])
                        {
                            Infos.Add(new MakeTemplateExpressionGenericArgPartGeneric(expression, i, j));
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        StealConstraint(i, GenericMakingConstraint.AllowTypes, expression);
                        Infos.Add(new GenericMakingDefinitionGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(FunctionDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.FunctionName is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new FunctionDefinitionNamePartGeneric(expression, i));
                    }
                    if (expression.ReturnTypeHint?.Typename is NameExpression ret_type && ret_type.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new FunctionDefinitionReturnTypePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j].Name.TypeHint?.Typename is NameExpression arg_name3 && !expression.Arguments[j].Name.TypeHint.is_array && arg_name3.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                            Infos.Add(new FunctionDefinitionArgumentTypePartGeneric(expression, i, j));
                        }
                        if (expression.Arguments[j].DefaultValue is NameExpression arg_name2 && arg_name2.Name == Generics[i])
                        {
                            Infos.Add(new FunctionDefinitionArgumentDefaultValuePartGeneric(expression, i, j));
                        }
                    }
                    for (var j = 0; j < expression.GenericArguments.Length; j++)
                    {
                        if (expression.GenericArguments[j].Item2 is NameConstraintDefinition arg_name2)
                        {
                            if (arg_name2.Name is NameExpression name2 && name2.Name == Generics[i])
                            {
                                CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowConstraints, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                                Infos.Add(new FunctionDefinitionGenericArgumentConstraintPartGeneric(expression, i, j));
                            }
                            if (arg_name2.Name is TypenameExpression name3 && name3.Typename is NameExpression name4 && name4.Name == Generics[i])
                            {
                                CheckAnyAllow(i, expression, GenericMakingConstraint.AllowValues, GenericMakingConstraint.AllowConstraints, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                                Infos.Add(new FunctionDefinitionGenericArgumentConstraintPartGeneric(expression, i, j));
                            }
                        }
                    }
                }
                var locked = expression.GenericArguments.Select(x => Array.FindIndex(Generics, y => y == x.Item1.Name)).ToArray();
                Shadowing.Push(locked);
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j].Name is NameExpression arg_name && arg_name.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames);
                            Infos.Add(new FunctionDefinitionArgumentNamePartGeneric(expression, i, j));
                        }
                    }
                }
                base.VisitExpression(expression);
                Shadowing.Pop();
            }

            public override void VisitExpression(BaseInstanceCreator expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Type?.Typename is NameExpression name1 && name1.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Type, GenericMakingConstraint.AllowTypes, GenericMakingConstraint.AllowNames);
                        Infos.Add(new BaseInstanceCreatorTypePartGeneric(expression, i));
                    }
                    if (expression.Name is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Name, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowExpressions);
                        Infos.Add(new BaseInstanceCreatorNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Arguments[j] is NameExpression name3 && name3.Name == Generics[i])
                            Infos.Add(new BaseInstanceCreatorArgPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorCollection expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    for (var j = 0; j < expression.Arguments.Length; j++)
                    {
                        if (expression.Body[j] is NameExpression name3 && name3.Name == Generics[i])
                            Infos.Add(new CreatorCollectionElementPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorContext expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Name is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Name, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowExpressions);
                        Infos.Add(new CreatorContextNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Ancestors.Length; j++)
                    {
                        if (expression.Ancestors[j] is NameExpression name3 && name3.Name == Generics[i])
                            Infos.Add(new CreatorContextAncestorPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorNewArray expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.ArrayType?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.ArrayType, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new CreatorNewArrayTypePartGeneric(expression, i));
                    }
                    if (expression.Size is NameExpression name2 && name2.Name == Generics[i])
                    {
                        Infos.Add(new CreatorNewArraySizePartGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorRange expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.RangeType?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.RangeType, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new CreatorRangeTypePartGeneric(expression, i));
                    }
                    if (expression.LowerBound is NameExpression name2 && name2.Name == Generics[i])
                        Infos.Add(new CreatorRangeBoundPartGeneric(expression, i, false));
                    if (expression.UpperBound is NameExpression name3 && name3.Name == Generics[i])
                        Infos.Add(new CreatorRangeBoundPartGeneric(expression, i, true));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorTuple expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    for (var j = 0; j < expression.Expressions.Length; j++)
                    {
                        if (expression.Expressions[j] is NameExpression name3 && name3.Name == Generics[i])
                            Infos.Add(new CreatorTupleElementPartGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorUsing expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Type?.Typename is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Type, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                        Infos.Add(new CreatorUsingGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(UsingExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Alias is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Alias, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowExpressions);
                        Infos.Add(new UsingExpressionAliasPartGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CreatorDictionary.DictionaryEntry expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Key is NameExpression name3 && name3.Name == Generics[i])
                        Infos.Add(new CreatorDictionaryEntryPartGeneric(expression, i, false));
                    if (expression.Value is NameExpression name4 && name4.Name == Generics[i])
                        Infos.Add(new CreatorDictionaryEntryPartGeneric(expression, i, true));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(MatchExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Matching is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new MatchExpressionHeadPartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.Matches.Length; j++)
                    {
                        for (var k = 0; k < expression.Matches[j].Length; k++)
                        {
                            if (expression.Matches[j][k] is NameExpression arg_name2 && arg_name2.Name == Generics[i])
                            {
                                Infos.Add(new MatchExpressionMatchesPartGeneric(expression, i, j, k));
                            }
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(AccordExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    for (var j = 0; j < expression.HeadAccords.Length; j++)
                    {
                        if (expression.HeadAccords[j] is NameExpression name && name.Name == Generics[i])
                        {
                            Infos.Add(new AccordExpressionHeadPartGeneric(expression, i, j));
                        }
                    }
                    for (var j = 0; j < expression.Accordings.Length; j++)
                    {
                        for (var k = 0; k < expression.Accordings[j].Length; k++)
                        {
                            if (expression.Accordings[j][k].Item1 is NameExpression arg_name2 && arg_name2.Name == Generics[i])
                            {
                                Infos.Add(new AccordExpressionMatchesPartGeneric(expression, i, j, k, false));
                            }
                            if (expression.Accordings[j][k].Item2 is NameConstraintDefinition arg_name3 && arg_name3.Name is NameExpression name2 && name2.Name == Generics[i])
                            {
                                Infos.Add(new AccordExpressionMatchesPartGeneric(expression, i, j, k, true));
                            }
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(StaticExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Right is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new StaticExpressionGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(ReferenceExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Expression is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new ReferenceExpressionGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(DereferenceExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Expression is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new DereferenceExpressionGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(MacrosDefinition expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Executable is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new MacrosDefinitionGeneric(expression, i));
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(ReflectionExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new ReflectionExpressionLeftPartGeneric(expression, i));
                    }
                    if (expression.Right is NameExpression arg_name && arg_name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression, GenericMakingConstraint.AllowNames);
                        Infos.Add(new ReflectionExpressionNamePartGeneric(expression, i));
                    }
                    for (var j = 0; j < expression.MethodGenericArguments.Length; j++)
                    {
                        if (expression.MethodGenericArguments[j]?.Typename is NameExpression arg_name3 && !expression.MethodGenericArguments[j].is_array && arg_name3.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.MethodGenericArguments[j], GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                            Infos.Add(new ReflectionExpressionGenericArgumentPartGeneric(expression, i, j));
                        }
                    }
                    for (var j = 0; j < expression.MethodArguments.Length; j++)
                    {
                        if (expression.MethodArguments[j]?.Typename is NameExpression arg_name3 && !expression.MethodArguments[j].is_array && arg_name3.Name == Generics[i])
                        {
                            CheckAnyAllow(i, expression.MethodArguments[j], GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowTypes);
                            Infos.Add(new ReflectionExpressionArgumentPartGeneric(expression, i, j));
                        }
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(InterpolatedString expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    for (var j = 0; j < expression.Expressions.Length; j++)
                    {
                        if (expression.Expressions[j] is NameExpression name3 && name3.Name == Generics[i])
                            Infos.Add(new InterpolatedStringGeneric(expression, i, j));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(ConstraintExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Name is NameExpression name && name.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Name, GenericMakingConstraint.AllowNames, GenericMakingConstraint.AllowExpressions);
                        Infos.Add(new ConstraintExpressionNamePartGeneric(expression, i));
                    }
                    if (expression.Target is NameExpression name2 && name2.Name == Generics[i])
                    {
                        CheckAnyAllow(i, expression.Target, GenericMakingConstraint.AllowNames);
                        Infos.Add(new ConstraintExpressionTargetPartGeneric(expression, i));
                    }
                }
                base.VisitExpression(expression);
            }

            public override void VisitExpression(SliceExpression expression)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (expression.Left is NameExpression name && name.Name == Generics[i])
                        Infos.Add(new SliceExpressionLeftPartGeneric(expression, i));
                    if (expression.LowerBound is NameExpression name2 && name2.Name == Generics[i])
                        Infos.Add(new SliceExpressionBoundPartGeneric(expression, i, false));
                    if (expression.UpperBound is NameExpression name3 && name3.Name == Generics[i])
                        Infos.Add(new SliceExpressionBoundPartGeneric(expression, i, true));
                }
                base.VisitExpression(expression);
            }

            public override void VisitStatement(ExpressionStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
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
                    if (IsShaded(i)) continue;
                    if (statement is ReturnStatement expr && expr.Expression is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new ReturnStatementGeneric(expr, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(ThrowStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.ThrowExpression is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new ThrowStatementGeneric(statement, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(TryStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.CatchVariable is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new TryStatementGeneric(statement, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(WhileLoopStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.Condition is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new WhileLoopStatementGeneric(statement, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(DoWhileLoopStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.Condition is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new DoWhileLoopStatementGeneric(statement, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(FiniteLoopStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.Iterations is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new FiniteLoopStatementGeneric(statement, i));
                    }
                }
                base.VisitStatement(statement);
            }

            public override void VisitStatement(ForeachLoopStatement statement)
            {
                for (var i = 0; i < Generics.Length; i++)
                {
                    if (IsShaded(i)) continue;
                    if (statement.Left is NameExpression name && name.Name == Generics[i])
                    {
                        Infos.Add(new ForeachLoopStatementGeneric(statement, i, false));
                    }
                    if (statement.Iterator is NameExpression name2 && name2.Name == Generics[i])
                    {
                        Infos.Add(new ForeachLoopStatementGeneric(statement, i, true));
                    }
                }
                base.VisitStatement(statement);
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
                    if (IsShaded(i)) continue;
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
                    if (method.ReturnType?.Typename is NameExpression retTypeName && retTypeName.Name == Generics[i])
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
            AllowConstraints = 0x10,
            AllowValues = 0x20,

            AllowAll = AllowNames | AllowTypes | AllowExpressions | AllowCode | AllowConstraints | AllowValues,
        }
        public enum GenericMaking
        {
            AsValue = 0,
            AsType = 1,
            AsName = 2,
            AsExpression = 3,
            AsCode = 4,
            AsConstraint = 5,

            Constraint = 6,
            Runtime = 7,
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

        public TemplateMethod(string name, string[] paramNames, StatementList statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ContextWrap definitionPlace, bool @implicit, bool recursive, bool without_params, BaseExpression[] default_values, bool[] constants, (NameExpression, Constraint)[] generics) : base(name, paramNames, statements, paramTypes, returnType, definitionPlace, @implicit, recursive, without_params, default_values, constants)
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
                    throw new RuntimeError(string.Format(Locale.Current["ERR_DoesntFitConstraint"], args[i].Item2?.GetType().GetTypeString() ?? "null", args[i].Item2, Generics[i].Item2), Generics[i].Item2.IISourceContext);
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
            if (Statements.IsAbstract())
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
            return new TemplateMethod(name, ParamNames?.CloneArray(), OriginalStatements.CloneCast(), OriginalParamTypes.CloneArray(), OriginalReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, OriginalDefaultValues.CloneArray(), ContantsParams.Copy(), Generics.ConvertAll(x => (x.Item1.CloneCast(), x.Item2.CloneCast())))
            {
                Abstract = Abstract
            };
        }
        public Method CreateMethod()
        {
            return new Method(Name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive, WithoutParams, DefaultValues.CloneArray(), ContantsParams.Copy())
            {
                Abstract = Abstract
            };
        }
    }
}
