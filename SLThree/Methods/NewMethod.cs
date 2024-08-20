using System;
using System.Globalization;
using System.Reflection;

namespace SLThree.Methods
{
    public class NewMethod : MethodInfo
    {
        public override Type ReturnType => base.ReturnType;

        public override bool IsGenericMethod => base.IsGenericMethod;

        public override Delegate CreateDelegate(Type delegateType)
        {
            return base.CreateDelegate(delegateType);
        }
        public override Delegate CreateDelegate(Type delegateType, object target)
        {
            return base.CreateDelegate(delegateType, target);
        }

        public override Type[] GetGenericArguments()
        {
            return base.GetGenericArguments();
        }

        public override MethodInfo GetGenericMethodDefinition()
        {
            return base.GetGenericMethodDefinition();
        }

        public override bool ContainsGenericParameters => base.ContainsGenericParameters;
        public override bool IsGenericMethodDefinition => base.IsGenericMethodDefinition;
        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            return base.MakeGenericMethod(typeArguments);
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

        public override MethodAttributes Attributes => throw new NotImplementedException();

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override string Name => throw new NotImplementedException();

        public override Type DeclaringType => throw new NotImplementedException();

        public override Type ReflectedType => throw new NotImplementedException();
    }
}
