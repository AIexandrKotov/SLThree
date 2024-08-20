using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class MacrosDefinition : BaseExpression
    {
        public ExecutionContext.IExecutable Executable;

        public MacrosDefinition(ExecutionContext.IExecutable executable, SourceContext context) : base(context)
        {
            Executable = executable;
        }

        public override string ExpressionToString() => "$" + (Executable is BaseStatement ? $"{{{Executable}}}" : $"({Executable})");

        public override object GetValue(ExecutionContext context) => Executable.Clone();

        public override object Clone()
        {
            return new MacrosDefinition(Executable.CloneCast(), SourceContext.CloneCast());
        }
    }
}
