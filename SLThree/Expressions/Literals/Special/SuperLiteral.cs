using Pegasus.Common;

namespace SLThree
{
    public class SuperLiteral : Special
    {
        public SuperLiteral(SourceContext context) : base(context) { }
        public SuperLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "super";
        public override object GetValue(ExecutionContext context) => context.super;
        public override object Clone() => new SuperLiteral(SourceContext);
    }
}
