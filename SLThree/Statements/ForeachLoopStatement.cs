using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class ForeachLoopStatement : BaseStatement
    {
        public NameExpression Name { get; set; }
        public BaseExpression Iterator { get; set; }
        public BaseStatement[] LoopBody { get; set; }

        public ForeachLoopStatement() : base() { }
        public ForeachLoopStatement(NameExpression name, BaseExpression iterator, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
        {
            Name = name;
            Iterator = iterator;
            LoopBody = cycleBody.Statements.ToArray();
            count = LoopBody.Length;
        }

        private ExecutionContext last_context;
        private int variable_index;
        private int count;
        public override object GetValue(ExecutionContext context)
        {
            var iterator = Iterator.GetValue(context).Cast<IEnumerable>();
            if (context != last_context)
            {
                last_context = context;
                variable_index = context.LocalVariables.SetValue(Name.Name, null);
            }
            var ret = default(object);
            context.StartCycle();
            foreach (var x in iterator)
            {
                context.LocalVariables.SetValue(variable_index, x);
                for (var i = 0; i < count; i++)
                {
                    ret = LoopBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
                if (context.Returned || context.Broken) break;
            }
            context.EndCycle();
            return ret;
        }

        public override string ToString() => $"foreach ({Name} in {Iterator}) {{{LoopBody}}}";

        public override object Clone()
        {
            return new ForeachLoopStatement()
            {
                Name = Name.CloneCast(),
                Iterator = Iterator.CloneCast(),
                LoopBody = LoopBody.CloneArray(),
                SourceContext = SourceContext.CloneCast(),
                count = count
            };
        }
    }
}
