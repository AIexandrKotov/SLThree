

namespace SLThree
{
    public class UpperLiteral : Special
    {
        public UpperLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "upper";
        public override object GetValue(ExecutionContext context) => context.PreviousContext;
        public override object Clone() => new UpperLiteral(SourceContext);
    }
}
