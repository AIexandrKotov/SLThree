using Pegasus.Common;

namespace SLThree
{
    public class UpperLiteral : BaseExpression
    {
        public UpperLiteral(SourceContext context) : base(context) { }
        public UpperLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "upper";
        public override object GetValue(ExecutionContext context) => context.PreviousContext.wrap;
        public override object Clone() => new UpperLiteral(SourceContext);
    }
}
