using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryNot : ExpressionUnary
    {
        public override string Operator => "!";
        public ExpressionUnaryNot(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryNot() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).CastToMax();
            switch (left)
            {
                case long v: return v == 0 ? 1 : 0;
                case ulong v: return v == 0 ? 1 : 0;
                case double v: return v == 0 ? 1 : 0;
            }
            throw new UnsupportedTypesInUnaryExpression(this, left?.GetType());
        }
    }
}
