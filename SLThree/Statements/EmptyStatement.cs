using SLThree.Extensions.Cloning;

namespace SLThree
{
    internal class EmptyStatement : BaseStatement
    {
        public EmptyStatement(SourceContext context) : base(context) { }

        public override string ToString() => $";";
        public override object GetValue(ExecutionContext context) => null;
        public override object Clone() => new EmptyStatement(SourceContext.CloneCast());
    }
}
