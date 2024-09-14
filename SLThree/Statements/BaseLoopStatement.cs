using System.Linq;

namespace SLThree
{
    public abstract class BaseLoopStatement : BaseStatement
    {
        public BaseStatement[] LoopBody;
        protected int count;
        public BaseLoopStatement()
        {
        }

        public BaseLoopStatement(StatementList cycleBody, ISourceContext context) : base(context)
        {
            LoopBody = cycleBody.Statements.ToArray();
            count = LoopBody.Length;
        }

        public BaseLoopStatement(BaseStatement[] cycleBody, ISourceContext context) : base(context)
        {
            LoopBody = cycleBody;
            count = LoopBody.Length;
        }
    }
}
