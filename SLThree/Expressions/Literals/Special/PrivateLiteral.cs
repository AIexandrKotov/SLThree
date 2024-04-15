using Pegasus.Common;

namespace SLThree
{
    public class PrivateLiteral : Special
    {
        public PrivateLiteral(SourceContext context) : base(context) { }
        public PrivateLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "private";
        public override object GetValue(ExecutionContext context) => context.@this.Context.@private;
        public override object Clone() => new PrivateLiteral(SourceContext);
    }
}
