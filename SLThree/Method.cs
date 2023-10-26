using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SLThree
{
    public class Method
    {
        private static Dictionary<Method, ExecutionContext> cached_method_contextes = new Dictionary<Method, ExecutionContext>();
        
        public string Name;
        public string[] ParamNames;
        public StatementListStatement Statements;

        private ExecutionContext GetExecutionContext(SLTSpeedyObject[] arguments, ExecutionContext context = null)
        {
            if (cached_method_contextes.TryGetValue(this, out var cntx))
            {
                cntx.PrepareToInvoke();
                cntx.PreviousContext = context;
                cntx.LocalVariables.FillArguments(this, arguments);
                return cntx;
            }
            else
            {
                var ret = new ExecutionContext();
                ret.LocalVariables.FillArguments(this, arguments);
                cached_method_contextes[this] = ret;
                return ret;
            }
        }

        public virtual object GetValue(SLTSpeedyObject[] args) => GetValue(null, args);

        public virtual object GetValue(ExecutionContext old_context, SLTSpeedyObject[] args)
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
