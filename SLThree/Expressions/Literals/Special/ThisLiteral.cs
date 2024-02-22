using Pegasus.Common;

namespace SLThree
{
    public class ThisLiteral : Special
    {
        public ThisLiteral(SourceContext context) : base(context) { }
        public ThisLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "this";

        public override object GetValue(ExecutionContext context) => context.@this;
        public override object Clone() => new ThisLiteral(SourceContext);
    }
}
