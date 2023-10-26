using Pegasus.Common;

namespace SLThree
{
    public class ExpressionBinaryAssign : ExpressionBinary
    {
        public override string Operator => "=";
        public ExpressionBinaryAssign(BoxSupportedLexem left, BoxSupportedLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryAssign() : base() { }
        private ExecutionContext counted_invoked;
        private bool is_namelexem;
        private int variable_index;
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var right = Right.GetBoxValue(context);
            if (counted_invoked == context && is_namelexem)
            {
                context.LocalVariables.SetValue(variable_index, right);
                reference = SLTSpeedyObject.GetAny(right);
                return ref reference;
            }
            else
            {
                if (Left is MemberAccess memberAccess)
                {
                    memberAccess.SetValue(context, right);
                    reference = SLTSpeedyObject.GetAny(right);
                    return ref reference;
                }
                else if (Left is NameLexem nl)
                {
                    variable_index = context.LocalVariables.SetValue(nl.Name, right);
                    is_namelexem = true;
                    counted_invoked = context;
                    reference = SLTSpeedyObject.GetAny(right);
                    return ref reference;
                }
            }
            throw new UnsupportedTypesInBinaryExpression(this, Left?.GetType(), right.Boxed()?.GetType());
        }
    }
}
