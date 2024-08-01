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
        public AbstractInvokation(SourceContext context) : base(GetErrorText(), context) { }

        public static string GetErrorText()
        {
            return $"Trying to invoke abstract method";
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
            : base($"It is impossible to make generic {making} in {gi.GetType().GetTypeString()}", expression?.SourceContext) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterName : RuntimeError
    {
        public UnavailableGenericParameterName() : base() { }
        public UnavailableGenericParameterName(TemplateMethod.GenericMaking making, string parameter)
            : base($"It is impossible to make generic {making} for {parameter} parameter name", null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterType : RuntimeError
    {
        public UnavailableGenericParameterType() : base() { }
        public UnavailableGenericParameterType(TemplateMethod.GenericMaking making, string parameter)
            : base($"It is impossible to make generic {making} for {parameter} parameter type", null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericResultType : RuntimeError
    {
        public UnavailableGenericResultType() : base() { }
        public UnavailableGenericResultType(TemplateMethod.GenericMaking making)
            : base($"It is impossible to make generic {making} for result type", null) { }
    }
    /// <summary>
    /// Недоступная Generic-подстановка
    /// </summary>
    [Serializable]
    public class UnavailableGenericParameterDefaultValue : RuntimeError
    {
        public UnavailableGenericParameterDefaultValue() : base() { }
        public UnavailableGenericParameterDefaultValue(TemplateMethod.GenericMaking making, string parameter)
            : base($"It is impossible to make generic {making} for {parameter} parameter defaultvalue", null) { }
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
            sb.Append($"{executable.GetType().GetTypeString()} does not support constraints imposed on T");
            if (gains.Any())
                sb.Append($". Please note that the following constraints were added automatically: [" + gains.Select(x => $"{x.Item1} => {x.Item2} at {TakeContext(x.Item3)}").JoinIntoString(", ") + "]");
            return sb.ToString();
        }

        public static SourceContext TakeContext(ExecutionContext.IExecutable executable)
        {
            if (executable is BaseExpression expr) return expr.SourceContext;
            if (executable is BaseStatement code) return code.SourceContext;
            return null;
        }
    }
}
