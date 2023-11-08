using Pegasus.Common;
using SLThree.Extensions;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class ForeachLoopStatement : BaseStatement
    {
        public NameLexem Name { get; set; }
        public BaseLexem Iterator { get; set; }
        public BaseStatement[] CycleBody { get; set; }

        public ForeachLoopStatement(NameLexem name, BaseLexem iterator, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
        {
            Name = name;
            Iterator = iterator;
            CycleBody = cycleBody.Statements.ToArray();
            count = CycleBody.Length;
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
                    ret = CycleBody[i].GetValue(context);
                    if (context.Returned || context.Broken) break;
                    if (context.Continued) continue;
                }
            }
            context.EndCycle();
            return ret;
        }

        public override string ToString() => $"foreach ({Name} in {Iterator}) {{{CycleBody}}}";
    }
}
