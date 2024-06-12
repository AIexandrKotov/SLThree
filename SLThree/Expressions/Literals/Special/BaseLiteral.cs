using Pegasus.Common;

namespace SLThree
{
    public class BaseLiteral : Special
    {
        public BaseLiteral(SourceContext context) : base(context) { }
        public BaseLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "base";
        public override object GetValue(ExecutionContext context) => context.@this.Context.@base;
        public override object Clone() => new BaseLiteral(SourceContext);
    }
}
