using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class NullLiteral : BaseLexem
    {
        public NullLiteral() : base() { }
        public NullLiteral(SourceContext context) : base(context) { }

        public override string ToString() => "null";

        public override object GetValue(ExecutionContext context) => null;

        public override object Clone()
        {
            return new NullLiteral(SourceContext.CloneCast());
        }
    }
}
