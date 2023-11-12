using Pegasus.Common;

namespace SLThree
{
    public class GlobalLiteral : BaseLexem
    {
        public GlobalLiteral(SourceContext context) : base(context) { }
        public GlobalLiteral(Cursor cursor) : base(cursor) { }

        public override string ToString() => "global";
        public override object GetValue(ExecutionContext context) => ExecutionContext.global;
        public override object Clone() => new GlobalLiteral(SourceContext);
    }
}
