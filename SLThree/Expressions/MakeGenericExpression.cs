using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SLThree
{
    public class MakeGenericExpression : BaseExpression
    {
        public BaseExpression Left;
        public TypenameExpression[] GenericArguments;
        private bool null_conditional;
        public bool NullConditional => null_conditional;

        public MakeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, ISourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
        }
        public MakeGenericExpression(BaseExpression left, TypenameExpression[] genericArguments, bool null_conditional, ISourceContext context) : base(context)
        {
            Left = left;
            GenericArguments = genericArguments;
            this.null_conditional = null_conditional;
        }

        public override string ExpressionToString() => $"{Left}{(null_conditional ? ".?" : "")}<{GenericArguments.JoinIntoString(", ")}>";

        public object InvokeForObj(ExecutionContext context, Type[] generic_args, object o)
        {
            if (o == null)
            {
                if (null_conditional) return null;
                throw new MethodNotFound(Left, SourceContext);
            }

            if (o is Method method)
            {
                if (o is GenericMethod generic_method)
                    return generic_method.MakeGenericMethod(generic_args);

                throw new RuntimeError("Generic invokation for SLThree methods is not supported", SourceContext);
            }
            else if (o is MethodInfo mi)
            {
                if (!mi.IsStatic) return mi.MakeGenericMethod(generic_args);
                else return mi.MakeGenericMethod(generic_args);
            }
            else
            {
                var type = o.GetType();
                return type.GetMethods()
                    .FirstOrDefault(x => x.Name == Left.ExpressionToString())
                    ?.MakeGenericMethod(generic_args);
            }

            throw new MakeNotAllow(o.GetType(), SourceContext);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(ExecutionContext context, Type[] generic_args)
        {
            return InvokeForObj(context, generic_args, Left.GetValue(context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object GetValue(ExecutionContext context)
        {
            return GetValue(context, GenericArguments.ConvertAll(x => (Type)x.GetValue(context)));
        }

        private bool cached_1;
        private MethodInfo founded;
        public object GetValue(ExecutionContext context, object obj)
        {
            var key = Left.Cast<NameExpression>().Name;
            var generic_args = GenericArguments.ConvertAll(x => (Type)x.GetValue(context));

            if (cached_1) return founded.MakeGenericMethod(generic_args);

            if (obj is ClassAccess ca)
            {
                ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                // после первого вызова GetMethod
                // переставляет перегрузки, у которых аргумент object
                // в начало массива методов
                founded = ca.Name.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).FirstOrDefault(x => x.Name == key);
                cached_1 = true;
                if (founded == null) throw new MethodNotFound(key, SourceContext);
                return founded.MakeGenericMethod(generic_args);
            }
            else if (obj != null)
            {
                var method = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name == key);
                if (method != null) return method.MakeGenericMethod(generic_args);
                else return InvokeForObj(context, generic_args, MemberAccess.GetNameExprValue(context, obj, Left as NameExpression));
            }

            return null;
        }

        public override object Clone()
        {
            return new MakeGenericExpression(Left.CloneCast(), GenericArguments.CloneArray(), null_conditional, SourceContext.CloneCast());
        }
    }
}
