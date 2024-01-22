using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Linq.Expressions;

namespace SLThree
{
    public class ContinueStatement : BaseStatement
    {
        public ContinueStatement(Cursor cursor) : base(cursor) { }
        public ContinueStatement(SourceContext context) : base(context) { }

        public override string ToString() => $"continue";
        public override object GetValue(ExecutionContext context)
        {
            context.Continue();
            return null;
        }
        public override object Clone() => new ContinueStatement(SourceContext.CloneCast());
    }
}
