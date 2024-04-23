using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class BinaryIsAssign : BinaryOperator
    {
        public BaseExpression Variable;
        public BinaryIsAssign(BaseExpression left, BaseExpression right, BaseExpression variable, SourceContext context) : base(left, right, context)
        {
            Variable = variable;
        }

        public override string Operator => "is";

        public override string ExpressionToString()
        {
            return $"{Left} is {Right}{(Variable != null?$" {Variable}":"")}";
        }

        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int var_index;
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context);
            var type = Right.GetValue(context).Cast<Type>();
            var is_type = left.GetType().IsType(type);
            if (is_type)
                BinaryAssign.AssignToValue(context, Variable, left, ref counted_invoked, ref is_name_expr, ref var_index);
            return is_type;
        }

        public override object Clone()
        {
            return new BinaryIsAssign(Left.CloneCast(), Right.CloneCast(), Variable.CloneCast(), SourceContext.CloneCast());
        }
    }
}
