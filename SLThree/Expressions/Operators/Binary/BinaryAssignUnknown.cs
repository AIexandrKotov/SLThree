using SLThree.Extensions.Cloning;
using System.Reflection;

namespace SLThree
{
    public class BinaryAssignUnknown : UnaryOperator
    {
        public override string Operator => "? =";
        public BinaryAssignUnknown(BaseExpression left, SourceContext context, bool priority = false) : base(left, context, priority) { }
        public BinaryAssignUnknown() : base() { }

        public override object GetValue(ExecutionContext context)
        {
            var right = Left.GetValue(context);
            if (right != null)
            {
                if (right is Method method)
                {
                    context.LocalVariables.SetValue(method.Name, method);
                    return method;
                }
                if (right is MethodInfo methodInfo)
                {
                    context.LocalVariables.SetValue(methodInfo.Name, methodInfo);
                    return methodInfo;
                }
                if (right is ContextWrap wrap)
                {
                    context.LocalVariables.SetValue(wrap.Context.Name, wrap);
                    return wrap;
                }
                /*NonGenericWrapper wrapper;
                if ((wrapper = NonGenericWrapper.GetWrapper(right.GetType())).HasName())
                {
                    var name = wrapper.GetName(right);
                    context.LocalVariables.SetValue(name, right);
                    return right;
                }*/
            }
            throw new OperatorError(this, right?.GetType() ?? Left.GetType());
        }

        public override object Clone()
        {
            return new BinaryAssignUnknown(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
