using Pegasus.Common;

namespace SLThree
{
    public class GlobalLiteral : BaseLexem
    {
        public GlobalLiteral(SourceContext context) : base(context) { }
        public GlobalLiteral(Cursor cursor) : base(cursor) { }

        public override string LexemToString() => "global";
        public override object GetValue(ExecutionContext context) => ExecutionContext.global;
        public override object Clone() => new GlobalLiteral(SourceContext);
    }
    public class UpperLiteral : BaseLexem
    {
        public UpperLiteral(SourceContext context) : base(context) { }
        public UpperLiteral(Cursor cursor) : base(cursor) { }
        
        public override string LexemToString() => "upper";
        public override object GetValue(ExecutionContext context) => context.upper;
        public override object Clone() => new UpperLiteral(SourceContext);
    }
}
