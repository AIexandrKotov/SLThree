using Pegasus.Common;

namespace SLThree
{
    public class ParentLiteral : Special
    {
        public ParentLiteral(SourceContext context) : base(context) { }
        public ParentLiteral(Cursor cursor) : base(cursor) { }

        public override string ExpressionToString() => "parent";
        public override object GetValue(ExecutionContext context) => context.@this.Context.parent;
        public override object Clone() => new ParentLiteral(SourceContext);
    }
}
