

namespace SLThree
{
    public class PrivateLiteral : Special
    {
        public PrivateLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "private";
        public override object GetValue(ExecutionContext context) => context.@this.Context.@private;
        public override object Clone() => new PrivateLiteral(SourceContext);
    }
}
