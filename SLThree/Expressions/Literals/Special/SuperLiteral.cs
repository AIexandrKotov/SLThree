

namespace SLThree
{
    public class SuperLiteral : Special
    {
        public SuperLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "super";
        public override object GetValue(ExecutionContext context) => context.super;
        public override object Clone() => new SuperLiteral(SourceContext);
    }
}
