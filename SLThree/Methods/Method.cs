using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace SLThree
{
    public class Method : ICloneable
    {
        private static Dictionary<Method, ExecutionContext> cached_method_contextes = new Dictionary<Method, ExecutionContext>();
        
        public string Name;
        public string[] ParamNames;
        public StatementListStatement Statements;

        public ExecutionContext.ContextWrap DefinitionPlace;

        public bool imp = false;

        public string contextName = "";
        public void UpdateContextName() => contextName = $"<{Name}>methodcontext";

        public override string ToString() => $"_ {Name}({ParamNames.ConvertAll(x => "_").JoinIntoString(", ")})";

        public virtual ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext context = null)
        {
            ExecutionContext ret;
            if (cached_method_contextes.TryGetValue(this, out var cntx))
            {
                ret = cntx;
                ret.PrepareToInvoke();
            }
            else
            {
                ret = new ExecutionContext(context);
                ret.@this = DefinitionPlace;
                ret.toplevel = ret.@this.pred?.toplevel;
                cached_method_contextes.Add(this, ret);
            }
            ret.Name = contextName;
            ret.PreviousContext = context;
            ret.LocalVariables.FillArguments(this, arguments);
            ret.fimp = !imp;
            return ret;
        }

        public virtual object GetValue(object[] args) => GetValue(null, args);

        public virtual object GetValue(ExecutionContext old_context, object[] args)
        {
            var context = GetExecutionContext(args, old_context);
            for (var i = 0; i < Statements.Statements.Length; i++)
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

        public virtual object Clone()
        {
            return new Method()
            {
                DefinitionPlace = DefinitionPlace,
                imp = imp,
                Name = Name,
                ParamNames = ParamNames.CloneArray(),
                Statements = Statements.CloneCast()
            };
        }
    }
}
