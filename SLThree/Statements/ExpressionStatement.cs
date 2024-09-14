using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ExpressionStatement : BaseStatement
    {
        public BaseExpression Expression;

        public ExpressionStatement(BaseExpression expression, ISourceContext context) : base(context)
        {
            Expression = expression;
        }

        public override string ToString() => $"{Expression}";
        public override object GetValue(ExecutionContext context) => Expression.GetValue(context);

        public override object Clone()
        {
            return new ExpressionStatement(Expression.CloneCast(), SourceContext.CloneCast());
        }
    }
}
