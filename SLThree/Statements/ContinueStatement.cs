
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ContinueStatement : BaseStatement
    {
        public ContinueStatement(ISourceContext context) : base(context) { }

        public override string ToString() => $"continue";
        public override object GetValue(ExecutionContext context)
        {
            context.Continue();
            return null;
        }
        public override object Clone() => new ContinueStatement(SourceContext.CloneCast());
    }
}
