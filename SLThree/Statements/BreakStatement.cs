
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class BreakStatement : BaseStatement
    {
        public BreakStatement(ISourceContext context) : base(context) { }

        public override string ToString() => $"break";
        public override object GetValue(ExecutionContext context)
        {
            context.Break();
            return null;
        }
        public override object Clone() => new BreakStatement(SourceContext.CloneCast());
    }
}
