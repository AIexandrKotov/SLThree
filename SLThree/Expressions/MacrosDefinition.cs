using SLThree.Extensions.Cloning;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override object GetValue(ExecutionContext context)
        {
            var nex = Executable is ICloneable cloneable ? (ExecutionContext.IExecutable)cloneable.Clone() : Executable;
            return nex;
        }

        public override object Clone()
        {
            return new MacrosDefinition(Executable is ICloneable cloneable ? (ExecutionContext.IExecutable)cloneable.Clone() : Executable, SourceContext.CloneCast());
        }
    }
}
