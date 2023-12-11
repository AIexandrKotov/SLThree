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
    public class UpperLiteral : BaseExpression
    {
        public UpperLiteral(SourceContext context) : base(context) { }
        public UpperLiteral(Cursor cursor) : base(cursor) { }
        
        public override string ExpressionToString() => "upper";
        public override object GetValue(ExecutionContext context) => context.upper;
        public override object Clone() => new UpperLiteral(SourceContext);
    }
}
