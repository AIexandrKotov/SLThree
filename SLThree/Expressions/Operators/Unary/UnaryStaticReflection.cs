using SLThree.Extensions;

namespace SLThree
{
    public class UnaryStaticReflection : UnaryOperator
    {
        public UnaryStaticReflection(BaseExpression left, ISourceContext context) : base(left, context)
        {
        }

        public override string Operator => "@@";

        public override object GetValue(ExecutionContext context)
        {
            return Left.Cast<TypenameExpression>().GetStaticValue();
        }

        public override object Clone()
        {
            return new UnaryReflection(Left, SourceContext);
        }
    }
}