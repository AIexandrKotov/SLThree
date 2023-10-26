using Pegasus.Common;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class SwitchStatement : BaseStatement
    {
        public class Node
        {
            public bool Next { get; set; }
            public BaseLexem Value { get; set; }
            public BaseStatement Statements { get; set; }

            public Node(BaseLexem condition, BaseStatement statements, bool next)
            {
                Value = condition;
                Statements = statements;
                Next = next;
            }

            public override string ToString() => $"case {Value}: {{{Statements}}}";
        }

        public BaseLexem Value;
        public IList<Node> Cases;

        public SwitchStatement(BaseLexem value, IList<Node> cases, Cursor cursor) : base(cursor)
        {
            Value = value;
            Cases = cases;
        }

        public override string ToString() => $"switch ({Value}) {{{Cases}}}";

        public override object GetValue(ExecutionContext context)
        {
            var value = Value.GetValue(context).Boxed();
            for (var i = 0; i < Cases.Count; i++)
            {
                var found = (value as IComparable)?.CompareTo(Cases[i].Value.GetValue(context)) == 0;
                var found_but_next = found && Cases[i].Next;
                if (found_but_next)
                {
                    while (i < Cases.Count && Cases[i].Next) i++;
                    if (i == Cases.Count) return null;
                    else return Cases[i].Statements.GetValue(context);
                }
                else if (found) return Cases[i].Statements.GetValue(context);
                else continue;
            }
            return null;
        }
    }
}
