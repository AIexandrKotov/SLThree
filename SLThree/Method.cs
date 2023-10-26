using System;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SLThree
{
    public class Method
    {
        public string Name;
        public string[] ParamNames;
        public StatementListStatement Statements;

        public ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext context = null)
        {
            var ret = new ExecutionContext();
            ret.PreviousContext = context;
            for (var i = 0; i < ParamNames.Length; i++)
                ret.LocalVariables.SetValue(ParamNames[i], arguments[i]);
            return ret;
        }

        public virtual object GetValue(object[] args) => GetValue(null, args);

        public virtual object GetValue(ExecutionContext old_context, object[] args)
        {
            var context = GetExecutionContext(args, old_context);
            for (var i = 0; i < Statements.Statements.Count; i++)
            {
                if (context.Returned) return context.ReturnedValue;
                else Statements.Statements[i].GetValue(context);
            }
            if (context.Returned) return context.ReturnedValue;
            return null;
        }

        public static MethodInfo Create<TResult>(Func<TResult> func) => func.Method;
        public static MethodInfo Create<T1, TResult>(Func<T1, TResult> func) => func.Method;
        public static MethodInfo Create<T1, T2, TResult>(Func<T1, T2, TResult> func) => func.Method;
        public static MethodInfo Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func) => func.Method;
        public static MethodInfo Create(Action func) => func.Method;
        public static MethodInfo Create<T1>(Action<T1> func) => func.Method;
        public static MethodInfo Create<T1, T2>(Action<T1, T2> func) => func.Method;
        public static MethodInfo Create<T1, T2, T3>(Action<T1, T2, T3> func) => func.Method;
        public static MethodInfo Create(Delegate func) => func.Method;
    }
}
