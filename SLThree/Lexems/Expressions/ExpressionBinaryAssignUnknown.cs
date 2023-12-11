using SLThree.Extensions.Cloning;
using System.Linq;
using System.Reflection;

namespace SLThree
{
    public class ExpressionBinaryAssignUnknown : ExpressionUnary
    {
        public override string Operator => "? =";
        public ExpressionBinaryAssignUnknown(BaseLexem left, SourceContext context, bool priority = false) : base(left, context, priority) { }
        public ExpressionBinaryAssignUnknown() : base() { }

        private static bool is_invalid_name(char x) => !(x == '_' || (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z'));

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
                if (right is ExecutionContext.ContextWrap wrap)
                {
                    context.LocalVariables.SetValue(wrap.pred.Name, wrap);
                    return wrap;
                }
                NonGenericWrapper wrapper;
                if ((wrapper = NonGenericWrapper.GetWrapper(right.GetType())).HasName())
                {
                    var name = wrapper.GetName(right);
                    context.LocalVariables.SetValue(name, right);
                    return right;
                }
            }
            throw new OperatorError(this, right?.GetType() ?? Left.GetType());
        }

        public override object Clone()
        {
            return new ExpressionBinaryAssignUnknown(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
