

namespace SLThree
{
    public class ThisLiteral : Special
    {
        public ThisLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "this";

        public override object GetValue(ExecutionContext context) => context.@this;
        public override object Clone() => new ThisLiteral(SourceContext);
    }
}
