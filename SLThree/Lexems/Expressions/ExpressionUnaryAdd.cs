using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryAdd : ExpressionUnary
    {
        public override string Operator => "+";
        public ExpressionUnaryAdd(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryAdd() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).CastToMax();
            switch (left)
            {
                case long v: return +v;
                case ulong v: return +v;
                case double v: return +v;
            }
            throw new UnsupportedTypesInUnaryExpression(this, left?.GetType());
        }
    }
}
