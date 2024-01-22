using Pegasus.Common;

namespace SLThree
{
    public class GlobalLiteral : BaseExpression
    {
        public GlobalLiteral(SourceContext context) : base(context) { }
        public GlobalLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "global";
        public override object GetValue(ExecutionContext context) => ExecutionContext.global;
        public override object Clone() => new GlobalLiteral(SourceContext);
    }
}
