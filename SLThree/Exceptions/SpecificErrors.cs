using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SLThree
{
    /// <summary>
    /// Исключения во время работы программы
    /// </summary>
    [Serializable]
    public class AbstractInvokation : RuntimeError
    {
        public AbstractInvokation() : base() { }
        public AbstractInvokation(ISourceContext context) : base(GetErrorText(), context) { }

        public static string GetErrorText()
        {
            return Locale.Current["ERR_AbstractInvokation"];
        }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericMaking : RuntimeError
    {
        public UnavailableGenericMaking() : base() { }
        public UnavailableGenericMaking(TemplateMethod.GenericMaking making, BaseExpression expression, object gi) 
            : base(string.Format(Locale.Current["ERR_T_Make"], making, gi.GetType().GetTypeString()), expression?.SourceContext) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterName : RuntimeError
    {
        public UnavailableGenericParameterName() : base() { }
        public UnavailableGenericParameterName(TemplateMethod.GenericMaking making, string parameter)
            : base(string.Format(Locale.Current["ERR_T_MakeParamName"], making, parameter), null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterType : RuntimeError
    {
        public UnavailableGenericParameterType() : base() { }
        public UnavailableGenericParameterType(TemplateMethod.GenericMaking making, string parameter)
            : base(string.Format(Locale.Current["ERR_T_MakeParamType"], making, parameter), null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericResultType : RuntimeError
    {
        public UnavailableGenericResultType() : base() { }
        public UnavailableGenericResultType(TemplateMethod.GenericMaking making)
            : base(string.Format(Locale.Current["ERR_T_MakeResultType"], making), null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterDefaultValue : RuntimeError
    {
        public UnavailableGenericParameterDefaultValue() : base() { }
        public UnavailableGenericParameterDefaultValue(TemplateMethod.GenericMaking making, string parameter)
            : base(string.Format(Locale.Current["ERR_T_MakeDefaultValue"], making, parameter), null) { }
    }
    /// <summary>
    /// Несогласованность ограничений
    /// </summary>
    [Serializable]
    public class ContraitConstraint : RuntimeError
    {
        public ContraitConstraint() : base() { }
        public ContraitConstraint(string generic_arg, TemplateMethod.GenericMakingConstraint making, ExecutionContext.IExecutable executable, IEnumerable<(TemplateMethod.GenericMakingConstraint, TemplateMethod.GenericMakingConstraint, ExecutionContext.IExecutable)> gains)
            : base(GetException(generic_arg, making, executable, gains), TakeContext(executable)) { }

        public static string GetException(string generic_arg, TemplateMethod.GenericMakingConstraint making, ExecutionContext.IExecutable executable, IEnumerable<(TemplateMethod.GenericMakingConstraint, TemplateMethod.GenericMakingConstraint, ExecutionContext.IExecutable)> gains)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format(Locale.Current["ERR_T_ContraintNotSupported"], executable?.GetType().GetTypeString() ?? "null", generic_arg));
            if (gains.Any())
                sb.Append($". {Locale.Current["ERR_T_AutoconstraintNote"]}: [" + gains.Select(x => $"{x.Item1} => {x.Item2}{AtContext(TakeContext(x.Item3))}").JoinIntoString(", ") + "]");
            return sb.ToString();
        }

        public static ISourceContext TakeContext(ExecutionContext.IExecutable executable)
        {
            if (executable is BaseExpression expr) return expr.SourceContext;
            if (executable is BaseStatement code) return code.SourceContext;
            return null;
        }
    }

    [Serializable]
    public class WrongCallArgumentsCount : RuntimeError
    {
        public WrongCallArgumentsCount() : base() { }
        public WrongCallArgumentsCount(ISourceContext context)
            : base(string.Format(Locale.Current["ERR_WrongArgs"]), context) { }
    }

    [Serializable]
    public class WrongConstructorCallArgumentsCount : RuntimeError
    {
        public WrongConstructorCallArgumentsCount() : base() { }
        public WrongConstructorCallArgumentsCount(ISourceContext context)
            : base(string.Format(Locale.Current["ERR_CtorWrongArgs"]), context) { }
    }

    [Serializable]
    public class CollectionIncorrectType : RuntimeError
    {
        public CollectionIncorrectType() : base() { }
        public CollectionIncorrectType(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_CollectionIncorrectType"], type?.GetTypeString()), context) { }
    }

    [Serializable]
    public class DictionaryIncorrectType : RuntimeError
    {
        public DictionaryIncorrectType() : base() { }
        public DictionaryIncorrectType(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_DictionaryIncorrectType"], type?.GetTypeString() ?? "null"), context) { }
    }

    [Serializable]
    public class RangeIncorrectType : RuntimeError
    {
        public RangeIncorrectType() : base() { }
        public RangeIncorrectType(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_RangeIncorrectType"], type?.GetTypeString() ?? "null"), context) { }
    }

    [Serializable]
    public class IsNotTuple : RuntimeError
    {
        public IsNotTuple() : base() { }
        public IsNotTuple(ISourceContext context)
            : base(string.Format(Locale.Current["ERR_IsNotTuple"]), context) { }
    }

    [Serializable]
    public class IndexTargetWasNull : RuntimeError
    {
        public IndexTargetWasNull() : base() { }
        public IndexTargetWasNull(object any, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_IndexTargetWasNull"], any), context) { }
    }

    [Serializable]
    public class MethodNotFound : RuntimeError
    {
        public MethodNotFound() : base() { }
        public MethodNotFound(BaseExpression left, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_MethodNotFound"], left), context) { }
        public MethodNotFound(string left, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_MethodNotFound"], left), context) { }
        public MethodNotFound(string methodName, int argCount, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_MethodOverloadNotFound"], methodName, Enumerable.Repeat("_", argCount).JoinIntoString(", ")), context) { }
    }

    [Serializable]
    public class InvokeNotAllow : RuntimeError
    {
        public InvokeNotAllow() : base() { }
        public InvokeNotAllow(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_InvokeNotAllow"], type.GetTypeString()), context) { }
    }

    [Serializable]
    public class MakeNotAllow : RuntimeError
    {
        public MakeNotAllow() : base() { }
        public MakeNotAllow(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_MakeNotAllow"], type.GetTypeString()), context) { }
    }

    [Serializable]
    public class NameNotFound : RuntimeError
    {
        public NameNotFound() : base() { }
        public NameNotFound(string name, Type target, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_NameNotFound"], name, target.GetTypeString()), context) { }
    }

    [Serializable]
    public class AssignToReadonly : RuntimeError
    {
        public AssignToReadonly() : base() { }
        public AssignToReadonly(BaseExpression right, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_AssignToReadonly"], right), context) { }
    }

    [Serializable]
    public class ReflectionNotFound : RuntimeError
    {
        public ReflectionNotFound() : base() { }
        public ReflectionNotFound(BaseExpression right, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_ReflectionNotFound"], right), context) { }
    }

    [Serializable]
    public class TypeNotFound : RuntimeError
    {
        public TypeNotFound() : base() { }
        public TypeNotFound(string type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_TypeNotFound"], type), context) { }
    }

    [Serializable]
    public class UnconvertibleToType : RuntimeError
    {
        public UnconvertibleToType() : base() { }
        public UnconvertibleToType(Type type, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_UnconvertibleToType"], type?.GetTypeString() ?? "null"), context) { }
    }

    [Serializable]
    public class GenericNotSupported : RuntimeError
    {
        public GenericNotSupported() : base() { }
        public GenericNotSupported(BaseExpression expression, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_GenericNotSupported"], expression.GetType()?.GetTypeString()), context) { }
    }

    [Serializable]
    public class RecursiveInsideNotSupported : RuntimeError
    {
        public RecursiveInsideNotSupported() : base() { }
        public RecursiveInsideNotSupported(ISourceContext context)
            : base(string.Format(Locale.Current["ERR_RecursiveInsideNotAllowed"]), context) { }
    }

    [Serializable]
    public class DefaultValuesNotDefined : RuntimeError
    {
        public DefaultValuesNotDefined() : base() { }
        public DefaultValuesNotDefined(ISourceContext context)
            : base(string.Format(Locale.Current["ERR_DefaultValuesNotDefined"]), context) { }
    }

    [Serializable]
    public class ConstraintNotFound : RuntimeError
    {
        public ConstraintNotFound() : base() { }
        public ConstraintNotFound(BaseExpression name, ISourceContext context)
            : base(string.Format(Locale.Current["ERR_ConstraintNotFound"], name), context) { }
    }
}
