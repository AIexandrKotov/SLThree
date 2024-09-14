

namespace SLThree
{
    public class SelfLiteral : Special
    {
        public SelfLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "self";

        public override object GetValue(ExecutionContext context) => context.wrap;
        public override object Clone() => new SelfLiteral(SourceContext);
    }
}
