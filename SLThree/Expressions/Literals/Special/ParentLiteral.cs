

namespace SLThree
{
    public class ParentLiteral : Special
    {
        public ParentLiteral(ISourceContext context) : base(context) { }

        public override string ExpressionToString() => "parent";
        public override object GetValue(ExecutionContext context) => context.@this.Context.parent;
        public override object Clone() => new ParentLiteral(SourceContext);
    }
}
