using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class InvokeGenericExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression[] GenericArguments;
        public BaseExpression[] Arguments;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        public InvokeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, BaseExpression[] arguments, ISourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
        }
        public InvokeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, BaseExpression[] arguments, bool null_conditional, ISourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            Arguments = arguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? ".?" : "")}<{GenericArguments.JoinIntoString(", ")}>({Arguments.JoinIntoString(", ")})";

        public object InvokeForObj(ExecutionContext context, Type[] generic_args, object[] args, object o)
        {
            if (o == null)
            {
                if (null_conditional) return null;
                throw new MethodNotFound(Left, SourceContext);
            }

            if (o is Method method)
            {
                if (args.Length < method.RequiredArguments || args.Length > method.MaximumArguments) throw new WrongCallArgumentsCount(SourceContext);
                if (o is GenericMethod generic_method)
                    return generic_method.MakeGenericMethod(generic_args).GetValue(context, args);
                if (o is TemplateMethod template_method)
                    return template_method.MakeGenericMethod(generic_args.ConvertAll(x => (TemplateMethod.GenericMaking.AsType, (object)x))).GetValue(context, args);

                throw new RuntimeError("Generic invokation for SLThree methods is not supported", SourceContext);
            }
            else if (o is ContextWrap wrap)
            {
                var lv = wrap.Context.LocalVariables.GetValue("constructor").Item1;
                if (lv is GenericMethod constructor)
                {
                    if (args.Length < constructor.RequiredArguments || args.Length > constructor.MaximumArguments) throw new WrongConstructorCallArgumentsCount(SourceContext);
                    return wrap.Context.CreateInstanceGeneric(context, constructor, generic_args, args).wrap;
                }
                else if (lv is TemplateMethod constructor2)
                {
                    if (args.Length < constructor2.RequiredArguments || args.Length > constructor2.MaximumArguments) throw new WrongConstructorCallArgumentsCount(SourceContext);
                    return wrap.Context.CreateInstanceTemplate(context, constructor2, generic_args.ConvertAll(x => (TemplateMethod.GenericMaking.AsType, (object)x)), args).wrap;
                }
                throw new RuntimeError($"Generic constructor not found", SourceContext);
            }
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.MakeGenericMethod(generic_args).Invoke(args[0], args.Skip(1).ToArray());
                else return mi.MakeGenericMethod(generic_args).Invoke(null, args);
            }
            else
            {
                var type = o.GetType();
                return type.GetMethods()
                    .FirstOrDefault(x => x.Name == Left.ExpressionToString().Replace(" ", "") && x.GetParameters().Length == Arguments.Length)
                    ?.MakeGenericMethod(generic_args)
                    .Invoke(o, args);
            }

            throw new InvokeNotAllow(o.GetType(), SourceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(ExecutionContext context, Type[] generic_args, object[] args)
        {
            return InvokeForObj(context, generic_args, args, Left.GetValue(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            if (obj is ClassAccess ca)
            {
                ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                // после первого вызова GetMethod
                // переставляет перегрузки, у которых аргумент object
                // в начало массива методов
                founded = ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length);
                cached_1 = true;
                if (founded == null) throw new MethodNotFound(key, Arguments.Length, SourceContext);
                return founded.MakeGenericMethod(generic_args).Invoke(null, Arguments.ConvertAll(x => x.GetValue(context)));
            }
            else if (obj != null)
            {
                var method = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key && x.GetParameters().Length == Arguments.Length);
                if (method != null) return method.MakeGenericMethod(generic_args).Invoke(obj, Arguments.ConvertAll(x => x.GetValue(context)));
                else return InvokeForObj(context, generic_args, Arguments.ConvertAll(x => x.GetValue(context)), MemberAccess.GetNameExprValue(context, obj, Left as NameExpression));
            }

            return null;
        }

        public override object Clone()
        {
            return new InvokeGenericExpression(Left.CloneCast(), GenericArguments.CloneArray(), Arguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}
