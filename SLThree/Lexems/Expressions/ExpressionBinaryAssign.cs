using Pegasus.Common;

namespace SLThree
{
    public class ExpressionBinaryAssign : ExpressionBinary
    {
        public override string Operator => "=";
        public ExpressionBinaryAssign(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryAssign() : base() { }
        private ExecutionContext counted_invoked;
        private bool is_namelexem;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            var right = Right.GetValue(context);
            if (counted_invoked == context && is_namelexem)
            {
                context.LocalVariables.SetValue(variable_index, right);
                return right;
            }
            else
            {
                if (Left is NameLexem nl)
                {
                    variable_index = context.LocalVariables.SetValue(nl.Name, right);
                    is_namelexem = true;
                    counted_invoked = context;
                    return right;
                }
                else if (Left is MemberAccess memberAccess)
                {
                    memberAccess.SetValue(context, right);
                    return right;
                }
                else if (Left is IndexLexem indexLexem)
                {
                    indexLexem.SetValue(context, right);
                    return right;
                }
            }
            context.Errors.Add(new OperatorError(this, Left?.GetType(), right?.GetType()));
            return null;
        }
    }
}
