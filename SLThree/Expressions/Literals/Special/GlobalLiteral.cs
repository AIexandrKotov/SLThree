

namespace SLThree
{
    public class GlobalLiteral : Special
    {
        public GlobalLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "global";
        public override object GetValue(ExecutionContext context) => ExecutionContext.global;
        public override object Clone() => new GlobalLiteral(SourceContext);
    }
}
