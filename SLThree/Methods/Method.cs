using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SLThree
{
    public class Method : ICloneable
    {
        private static readonly Dictionary<Method, ExecutionContext> cached_method_contextes = new Dictionary<Method, ExecutionContext>();

        public readonly string Name;
        public readonly string[] ParamNames;
        public readonly StatementList Statements;
        public readonly bool Implicit = false;
        public readonly bool Recursive = false;

        public TypenameExpression[] ParamTypes;
        public TypenameExpression ReturnType;

        internal ContextWrap definitionplace;
        internal string contextName = "";

        public ContextWrap DefinitionPlace => definitionplace;

        internal protected Method() { }
        public Method(string name, string[] paramNames, StatementList statements, TypenameExpression[] paramTypes, TypenameExpression returnType, ContextWrap definitionPlace, bool @implicit, bool recursive)
        {
            Name = name;
            ParamNames = paramNames;
            Statements = statements;
            ParamTypes = paramTypes;
            ReturnType = returnType;
            definitionplace = definitionPlace;
            Implicit = @implicit;
            Recursive = recursive;
        }

        internal void UpdateContextName() => contextName = $"<{Name}>methodcontext";

        public override string ToString() => $"{ReturnType?.ToString() ?? "any"} {Name}({ParamTypes.ConvertAll(x => x?.ToString() ?? "any").JoinIntoString(", ")})";

        public virtual ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext super_context = null)
        {
            ExecutionContext ret;
            if (Recursive)
            {
                ret = new ExecutionContext();
                ret.Name = contextName;
                ret.PreviousContext = super_context;
                ret.LocalVariables.FillArguments(this, arguments);
                ret.@this = definitionplace;
                ret.ForbidImplicit = !Implicit;
                return ret;
            }
            else
            {
                if (cached_method_contextes.TryGetValue(this, out var cntx))
                {
                    ret = cntx;
                    ret.PrepareToInvoke();
                }
                else
                {
                    ret = new ExecutionContext(super_context)
                    {
                        @this = definitionplace
                    };
                    ret.SuperContext = ret.@this.Context?.SuperContext;
                    cached_method_contextes.Add(this, ret);
                }
                ret.Name = contextName;
                ret.PreviousContext = super_context;
                ret.LocalVariables.FillArguments(this, arguments);
                ret.ForbidImplicit = !Implicit;
            }
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

        public object Invoke()
            => GetValue(new object[0]);
        public object Invoke<T>(T arg1)
            => GetValue(new object[1] { arg1 });
        public object Invoke<T1, T2>(T1 arg1, T2 arg2)
            => GetValue(new object[2] { arg1, arg2 });
        public object Invoke<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
            => GetValue(new object[3] { arg1, arg2, arg3 });
        public object Invoke(ExecutionContext old_context)
            => GetValue(old_context, new object[0]);
        public object Invoke<T>(ExecutionContext old_context, T arg1)
            => GetValue(old_context, new object[1] { arg1 });
        public object Invoke<T1, T2>(ExecutionContext old_context, T1 arg1, T2 arg2)
            => GetValue(old_context, new object[2] { arg1, arg2 });
        public object Invoke<T1, T2, T3>(ExecutionContext old_context, T1 arg1, T2 arg2, T3 arg3)
            => GetValue(old_context, new object[3] { arg1, arg2, arg3 });

        public static MethodInfo Create<TResult>(Func<TResult> func) => func.Method;
        public static MethodInfo Create<T1, TResult>(Func<T1, TResult> func) => func.Method;
        public static MethodInfo Create<T1, T2, TResult>(Func<T1, T2, TResult> func) => func.Method;
        public static MethodInfo Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func) => func.Method;
        public static MethodInfo Create(Action func) => func.Method;
        public static MethodInfo Create<T1>(Action<T1> func) => func.Method;
        public static MethodInfo Create<T1, T2>(Action<T1, T2> func) => func.Method;
        public static MethodInfo Create<T1, T2, T3>(Action<T1, T2, T3> func) => func.Method;
        public static MethodInfo Create(Delegate func) => func.Method;

        public virtual Method CloneWithNewName(string name)
        {
            return new Method(name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes?.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive);
        }

        public virtual object Clone()
        {
            return CloneWithNewName(Name);
        }
    }
}
