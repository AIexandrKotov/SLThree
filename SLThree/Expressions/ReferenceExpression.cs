using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ReferenceExpression : BaseExpression
    {
        public BaseExpression Expression;

        public ReferenceExpression(BaseExpression expression, ISourceContext context) : base(context)
        {
            Expression = expression;
        }

        public override string ExpressionToString() => $"&{Expression}";

        public override object GetValue(ExecutionContext context)
        {
            return new ContextualReference(Expression, context);
        }

        public override object Clone() => new ReferenceExpression(Expression.CloneCast(), SourceContext.CloneCast());
    }

    public class DereferenceExpression : BaseExpression
    {
        public BaseExpression Expression;

        public DereferenceExpression(BaseExpression expression, ISourceContext context) : base(context)
        {
            Expression = expression;
        }

        public override string ExpressionToString() => $"*{Expression}";

        public override object GetValue(ExecutionContext context)
        {
            return ((ContextualReference)Expression.GetValue(context)).GetValue();
        }

        public object SetValue(ExecutionContext context, object value)
        {
            return ((ContextualReference)Expression.GetValue(context)).SetValue(value);
        }

        public override object Clone() => new DereferenceExpression(Expression.CloneCast(), SourceContext.CloneCast());
    }
}
