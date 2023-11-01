using Pegasus.Common;
using SLThree.Extensions;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class ForeachCycleStatement : BaseStatement
    {
        public NameLexem Name { get; set; }
        public BaseLexem Iterator { get; set; }
        public StatementListStatement CycleBody { get; set; }

        public ForeachCycleStatement(NameLexem name, BaseLexem iterator, StatementListStatement cycleBody, Cursor cursor) : base(cursor)
        {
            Name = name;
            Iterator = iterator;
            CycleBody = cycleBody;
        }

        private ExecutionContext last_context;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            var iterator = Iterator.GetValue(context).Cast<IEnumerable>();
            var enumerator = iterator.GetEnumerator();
            if (context != last_context)
            {
                last_context = context;
                variable_index = context.LocalVariables.SetValue(Name.Name, null);
            }
            var ret = default(object);
            context.StartCycle();
            while (enumerator.MoveNext())
            {
                context.LocalVariables.SetValue(variable_index, enumerator.Current);
                for (var i = 0; i < CycleBody.Statements.Count; i++)
                {
                    ret = CycleBody.Statements[i].GetValue(context);
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
