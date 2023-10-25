using Pegasus.Common;

namespace SLThree
{
    public class ExpressionBinaryAssign : ExpressionBinary
    {
        public override string Operator => "=";
        public ExpressionBinaryAssign(BaseLexem left, BaseLexem right, Cursor cursor) : base(left, right, cursor) { }
        public ExpressionBinaryAssign() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var right = Right.GetValue(context);
            if (Left is MemberAccess memberAccess)
            {
                memberAccess.SetValue(context, right);
                return right;
            }
            else if (Left is NameLexem nl)
            {
                context.LocalVariables[nl.Name] = right;
                return right;
            }
            throw new UnsupportedTypesInBinaryExpression(this, Left?.GetType(), right?.GetType());
        }
    }
}
