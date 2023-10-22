using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryBitNot : ExpressionUnary
    {
        public override string Operator => "~";
        public ExpressionUnaryBitNot(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryBitNot() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).CastToMax();
            switch (left)
            {
                case long v: return ~v;
                case ulong v: return ~v;
            }
            throw new UnsupportedTypesInUnaryExpression(this, left?.GetType());
        }
    }
}
