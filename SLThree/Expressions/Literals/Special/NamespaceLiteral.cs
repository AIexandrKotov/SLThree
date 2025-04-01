namespace SLThree
{
    public class NamespaceLiteral : Special
    {
        public NamespaceLiteral(ISourceContext context) : base(context) { }
        public override string ExpressionToString() => "..";
        public override object GetValue(ExecutionContext context) => null;
        public override object Clone() => new NamespaceLiteral(SourceContext);
    }
}
