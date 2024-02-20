﻿using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class InvokeGenericExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression[] GenericArguments;
        public BaseExpression[] Arguments;
        private bool null_conditional;

        public InvokeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, BaseExpression[] arguments, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
        }
        public InvokeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, BaseExpression[] arguments, bool null_conditional, SourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? "?" : "")}<{GenericArguments.JoinIntoString(", ")}>({Arguments.JoinIntoString(", ")})";

        public object GetValue(ExecutionContext context, Type[] generic_args, object[] args)
        {
            var o = Left.GetValue(context);

            if (o == null)
            {
                if (null_conditional) return null;
                throw new RuntimeError($"Method `{Left}` not found", SourceContext);
            }

            if (o is Method method)
            {
                throw new NotSupportedException("Generic invokation for SLThree methods is not supported");
                if (method.ParamNames.Length != args.Length) throw new RuntimeError("Call with wrong arguments count", SourceContext);
                return method.GetValue(context, args);
            }
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.MakeGenericMethod(generic_args).Invoke(args[0], args.Skip(1).ToArray());
                else return mi.MakeGenericMethod(generic_args).Invoke(null, args);
            }
            else
            {
                var type = o.GetType();
                type.GetMethods()
                    .FirstOrDefault(x => x.Name == Left.ExpressionToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Length)
                    ?.MakeGenericMethod(generic_args)
                    .Invoke(o, args);
            }

            throw new RuntimeError($"{o.GetType().GetTypeString()} is not allow to invoke", SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, GenericArguments.ConvertAll(x => (Type)x.GetValue(context)), Arguments.ConvertAll(x => x.GetValue(context)));
        }

        private bool cached_1;
        private MethodInfo founded;
        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Left.Cast<NameExpression>().Name;
            var generic_args = GenericArguments.ConvertAll(x => (Type)x.GetValue(context));

            if (cached_1) return founded.MakeGenericMethod(generic_args).Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));

            if (obj is MemberAccess.ClassAccess ca)
            {
                ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static);
                // после первого вызова GetMethod
                // переставляет перегрузки, у которых аргумент object
                // в начало массива методов
                founded = ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length);
                cached_1 = true;
                if (founded == null) throw new RuntimeError($"Method `{key}({Arguments.Select(x => "_").JoinIntoString(", ")})` not found", SourceContext);
                return founded.MakeGenericMethod(generic_args).Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            else if (obj != null)
            {
                return obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length)
                    .MakeGenericMethod(generic_args)
                    .Invoke(obj, Arguments.ConvertAll(x => x.GetValue(context)));
            }

            return null;
        }

        public override object Clone()
        {
            return new InvokeGenericExpression(Left.CloneCast(), GenericArguments.CloneArray(), Arguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}