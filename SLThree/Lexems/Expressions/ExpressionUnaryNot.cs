using Pegasus.Common;
using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryNot : ExpressionUnary
    {
        public override string Operator => "!";
        public ExpressionUnaryNot(BoxSupportedLexem left, Cursor cursor) : base(left, cursor) { }
        public ExpressionUnaryNot() : base() { }
        public override ref SLTSpeedyObject GetBoxValue(ExecutionContext context)
        {
            var left = Left.GetBoxValue(context);

            if (left.Type == SLTSpeedyObject.BoolType)
            {
                reference.AsBool = !left.AsBool;
                return ref reference;
            }

            throw new UnsupportedTypesInUnaryExpression(this, left.Boxed()?.GetType());
        }
    }
}
