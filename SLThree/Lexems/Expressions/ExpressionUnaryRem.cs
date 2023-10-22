using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryRem : ExpressionUnary
    {
        public override string Operator => "-";
        public ExpressionUnaryRem(BaseLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryRem() : base() { }
        public override object GetValue(ExecutionContext context)
        {
            var left = Left.GetValue(context).CastToMax();
            switch (left)
            {
                case long v: return -v;
                case double v: return -v;
            }
            throw new UnsupportedTypesInUnaryExpression(this, left?.GetType());
        }
    }
}
