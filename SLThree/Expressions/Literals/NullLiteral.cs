using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class NullLiteral : Literal<object>
    {
        public NullLiteral() : base() { }
        public NullLiteral(SourceContext context) : base(null, "null", context) { }

        public override string ExpressionToString() => "null";

        public override object GetValue(ExecutionContext context) => null;

        public override object Clone()
        {
            return new NullLiteral(SourceContext.CloneCast());
        }
    }
}
