using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class EmptyStatement : BaseStatement
    {
        public EmptyStatement(ISourceContext context) : base(context) { }

        public override string ToString() => $";";
        public override object GetValue(ExecutionContext context) => null;
        public override object Clone() => new EmptyStatement(SourceContext.CloneCast());
    }
}
