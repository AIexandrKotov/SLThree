using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class SwitchStatement : BaseStatement
    {
        public class Node : ICloneable
        {
            public bool Next { get; set; }
            public BaseLexem Value { get; set; }
            public BaseStatement Statements { get; set; }

            public Node() { }
            public Node(BaseLexem condition, BaseStatement statements, bool next)
            {
                Value = condition;
                Statements = statements;
                Next = next;
            }

            public override string ToString() => $"case {Value}: {{{Statements}}}";

            public object Clone()
            {
                return new Node() { Next = Next.Copy(), Value = Value.CloneCast(), Statements = Statements.CloneCast() };
            }
        }

        public BaseLexem Value;
        public Node[] Cases;

        public SwitchStatement() : base() { }
        public SwitchStatement(BaseLexem value, IList<Node> cases, SourceContext context) : base(context)
        {
            Value = value;
            Cases = cases.ToArray();
        }

        public override string ToString() => $"switch ({Value}) {{{Cases}}}";

        public override object GetValue(ExecutionContext context)
        {
            var value = Value.GetValue(context);
            for (var i = 0; i < Cases.Length; i++)
            {
                var found = (value as IComparable)?.CompareTo(Cases[i].Value.GetValue(context)) == 0;
                var found_but_next = found && Cases[i].Next;
                if (found_but_next)
                {
                    while (i < Cases.Length && Cases[i].Next) i++;
                    if (i == Cases.Length) return null;
                    else return Cases[i].Statements.GetValue(context);
                }
                else if (found) return Cases[i].Statements.GetValue(context);
                else continue;
            }
            return null;
        }

        public override object Clone()
        {
            return new SwitchStatement() { Cases = Cases.CloneArray(), Value = Value.CloneCast(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
