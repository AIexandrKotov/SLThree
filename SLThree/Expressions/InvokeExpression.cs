﻿using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class InvokeExpression : BaseExpression
    {
        public BaseExpression Left;
        public BaseExpression[] Arguments;
        private bool null_conditional;

        public InvokeExpression(BaseExpression name, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Left = name;
            Arguments = arguments;
        }
        public InvokeExpression(BaseExpression name, BaseExpression[] arguments, bool null_conditional, SourceContext context) : base(context)
        {
            Left = name;
            Arguments = arguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? "?" : "")}({Arguments.JoinIntoString(", ")})";

        public object GetValue(ExecutionContext context, object[] args)
        {
            var o = Left.GetValue(context);

            if (o == null)
            {
                if (null_conditional) return null;
                throw new RuntimeError($"Method `{Left}` not found", SourceContext);
            }

            if (o is Method method)
            {
                if (method.ParamNames.Length != args.Length) throw new RuntimeError("Call with wrong arguments count", SourceContext);
                return method.GetValue(context, args);
            }
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.Invoke(args[0], args.Skip(1).ToArray());
                else return mi.Invoke(null, args);
            }
            else if (o is ExecutionContext.IExecutable bl) return bl.GetValue(context);
            else if (o is ConstructorInfo ci)
            {
                return ci.Invoke(args);
            }
            else
            {
                var type = o.GetType();
                type.GetMethods()
                    .FirstOrDefault(x => x.Name == Left.ExpressionToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Length)
                    ?.Invoke(o, args);
            }

            throw new RuntimeError($"{o.GetType().GetTypeString()} is not allow to invoke", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, Arguments.ConvertAll(x => x.GetValue(context)));
        }

        private bool cached_1;
        private MethodInfo founded;
        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Left.Cast<NameExpression>().Name;

            if (cached_1) return founded.Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));

            if (obj is ClassAccess ca)
            {
                ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static);
                // после первого вызова GetMethod
                // переставляет перегрузки, у которых аргумент object
                // в начало массива методов
                founded = ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length);
                cached_1 = true;
                if (founded == null) throw new RuntimeError($"Method `{key}({Arguments.Select(x => "_").JoinIntoString(", ")})` not found", SourceContext);
                return founded.Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length)
                    .Invoke(obj, Arguments.ConvertAll(x => x.GetValue(context)));
            }

            return null;
        }

        public override object Clone()
        {
            return new InvokeExpression(Left.CloneCast(), Arguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}
