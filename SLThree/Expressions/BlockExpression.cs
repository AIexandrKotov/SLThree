using SLThree.Extensions.Cloning;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class BlockExpression : BaseExpression
    {
        public BaseStatement[] Statements;
        private protected int count;

        public BlockExpression() : base() { }
        
        public BlockExpression(IList<BaseStatement> statements, SourceContext context) : base(context)
        {
            Statements = statements.Where(x => !(x is EmptyStatement)).ToArray();
            count = Statements.Length;
        }

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            for (var i = 0; i < count; i++)
            {
                ret = Statements[i].GetValue(context);
                if (context.Returned || context.Broken || context.Continued) break;
            }
            return ret;
        }

        public override object Clone()
        {
            return new BlockExpression() { Statements = Statements.CloneArray(), count = count.Copy(), SourceContext = SourceContext.CloneCast() };
        }

        public override string ExpressionToString() => $"{Statements.Length} statements";
    }
}
