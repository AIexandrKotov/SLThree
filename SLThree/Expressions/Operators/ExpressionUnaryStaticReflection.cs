using SLThree.Extensions;

namespace SLThree
{
    public class ExpressionUnaryStaticReflection : ExpressionUnary
    {
        public ExpressionUnaryStaticReflection(BaseExpression left, SourceContext context) : base(left, context)
        {
        }

        public override string Operator => "@@";

        public override object GetValue(ExecutionContext context)
        {
            return Left.Cast<TypenameExpression>().GetStaticValue();
        }

        public override object Clone()
        {
            return new ExpressionUnaryReflection(Left, SourceContext);
        }
    }
}