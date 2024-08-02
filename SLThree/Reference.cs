using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ContextualReference
    {
        public BaseExpression Value;
        public ExecutionContext Context;

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        public ContextualReference(BaseExpression value, ExecutionContext context)
        {
            Value = value;
            Context = context;
        }

        public object GetValue()
        {
            return Value.GetValue(Context);
        }
        public object SetValue(object any)
        {
            return BinaryAssign.AssignToValue(Context, Value, any, ref counted_invoked, ref is_name_expr, ref variable_index);
        }

        public override string ToString() => $"&{Value}";
    }
}
