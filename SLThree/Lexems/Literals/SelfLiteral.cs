using Pegasus.Common;

namespace SLThree
{
    public class SelfLiteral : BaseLexem
    {
        public SelfLiteral(SourceContext context) : base(context) { }
        public SelfLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "self";

        public override object GetValue(ExecutionContext context) => context.wrap;
        public override object Clone() => new SelfLiteral(SourceContext);
    }
}
