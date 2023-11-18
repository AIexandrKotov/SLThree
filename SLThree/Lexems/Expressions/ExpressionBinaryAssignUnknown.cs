using SLThree.Extensions.Cloning;
using System.Linq;

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
            if (right == null) throw new OperatorError(this, right?.GetType());
            if (right is Method method)
            {
                if (method.Name.Any(is_invalid_name)) throw new RuntimeError($"{method.Name} is not valid name for variable", SourceContext);
                context.LocalVariables.SetValue(method.Name, method);
                return method;
            }
            else if (right is ExecutionContext.ContextWrap wrap)
            {
                if (wrap.pred.Name.Any(is_invalid_name)) throw new RuntimeError($"{wrap.pred.Name} is not valid name for variable", SourceContext);
                context.LocalVariables.SetValue(wrap.pred.Name, wrap);
                return wrap;
            }
            else
            {
                var name = NonGenericWrapper.GetWrapper(right.GetType()).GetName(right);
                if (name.Any(is_invalid_name)) throw new RuntimeError($"{name} is not valid name for variable", SourceContext);
                context.LocalVariables.SetValue(name, right);
            }
            throw new OperatorError(this, right?.GetType());
        }

        public override object Clone()
        {
            return new ExpressionBinaryAssignUnknown(Left.CloneCast(), SourceContext.CloneCast(), PrioriryRaised);
        }
    }
}
