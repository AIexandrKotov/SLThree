using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace SLThree
{
    public class Method : ICloneable
    {
        public const string DefaultMethodName = "$method";

        private ExecutionContext cached_context;
        public string Name;
        public readonly string[] ParamNames;
        public readonly StatementList Statements;
        public readonly bool Implicit = false;
        public readonly bool Recursive = false;
        public bool Abstract = false;
        public bool Binded = false;

        public TypenameExpression[] ParamTypes;
        public TypenameExpression ReturnType;

        internal ContextWrap definitionplace;
        internal string contextName = "";

        public ContextWrap @this
        {
            get => definitionplace; internal set
            {
                definitionplace = value;
                cached_context = null;
            }
        }

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            var unnamed = Name == DefaultMethodName;
            if (Abstract)
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
            sb.Append($"({ParamTypes.ConvertAll(x => x?.ToString() ?? "any").JoinIntoString(", ")})");
            sb.Append($": {ReturnType?.ToString() ?? "any"}");
            return sb.ToString();
        }

        public virtual ExecutionContext GetExecutionContext(object[] arguments, ExecutionContext super_context = null)
        {
            ExecutionContext ret;
            if (Recursive)
            {
                ret = new ExecutionContext(false, false);
                ret.Name = contextName;
                ret.PreviousContext = super_context;
                ret.LocalVariables.FillArguments(this, arguments);
                ret.@this = definitionplace;
                ret.ForbidImplicit = !Implicit;
                return ret;
            }
            else
            {
                if (cached_context != null)
                {
                    ret = cached_context;
                    ret.PrepareToInvoke();
                }
                else
                {
                    ret = new ExecutionContext(super_context, false)
                    {
                        @this = definitionplace
                    };
                    ret.SuperContext = ret.@this.Context?.SuperContext;
                    cached_context = ret;
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
            var i = 0;
            var bs = Statements.Statements;
            var count = bs.Length;
            while (i < count)
            {
                if (context.Returned) return context.ReturnedValue;
                else bs[i++].GetValue(context);
            }
            if (context.Returned) return context.ReturnedValue;
            return null;
        }

        public Method identity(ContextWrap context)
        {
            @this = context;
            Binded = true;
            return this;
        }

        #region Invoke [auto-generated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke()
            => GetValue(new object[0] { });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1)
            => GetValue(new object[1] { arg1 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2)
            => GetValue(new object[2] { arg1, arg2 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3)
            => GetValue(new object[3] { arg1, arg2, arg3 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4)
            => GetValue(new object[4] { arg1, arg2, arg3, arg4 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5)
            => GetValue(new object[5] { arg1, arg2, arg3, arg4, arg5 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
            => GetValue(new object[6] { arg1, arg2, arg3, arg4, arg5, arg6 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
            => GetValue(new object[7] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
            => GetValue(new object[8] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
            => GetValue(new object[9] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
            => GetValue(new object[10] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
            => GetValue(new object[11] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
            => GetValue(new object[12] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
            => GetValue(new object[13] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14)
            => GetValue(new object[14] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14, object arg15)
            => GetValue(new object[15] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Invoke(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14, object arg15, object arg16)
            => GetValue(new object[16] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16 });
        #endregion

        #region InvokeWithContext [auto-generated]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from)
            => GetValue(from, new object[0] { });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1)
            => GetValue(from, new object[1] { arg1 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2)
            => GetValue(from, new object[2] { arg1, arg2 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3)
            => GetValue(from, new object[3] { arg1, arg2, arg3 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4)
            => GetValue(from, new object[4] { arg1, arg2, arg3, arg4 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5)
            => GetValue(from, new object[5] { arg1, arg2, arg3, arg4, arg5 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
            => GetValue(from, new object[6] { arg1, arg2, arg3, arg4, arg5, arg6 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
            => GetValue(from, new object[7] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
            => GetValue(from, new object[8] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
            => GetValue(from, new object[9] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10)
            => GetValue(from, new object[10] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11)
            => GetValue(from, new object[11] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12)
            => GetValue(from, new object[12] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13)
            => GetValue(from, new object[13] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14)
            => GetValue(from, new object[14] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14, object arg15)
            => GetValue(from, new object[15] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15 });
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object InvokeWithContext(ExecutionContext from, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, object arg11, object arg12, object arg13, object arg14, object arg15, object arg16)
            => GetValue(from, new object[16] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16 });

        #endregion

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
            return new Method(name, ParamNames?.CloneArray(), Statements.CloneCast(), ParamTypes?.CloneArray(), ReturnType.CloneCast(), definitionplace, Implicit, Recursive)
            {
                Abstract = Abstract
            };
        }

        public virtual object Clone()
        {
            return CloneWithNewName(Name);
        }
    }
}
