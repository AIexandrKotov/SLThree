using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Collections;
using System.Linq;

namespace SLThree
{
    public class ForeachLoopStatement : BaseLoopStatement
    {
        public BaseExpression Left;
        public BaseExpression Iterator;

        public ForeachLoopStatement() : base() { }
        public ForeachLoopStatement(BaseExpression left, BaseExpression iterator, BaseStatement[] cycleBody, SourceContext context) : base(cycleBody, context)
        {
            Left = left;
            Iterator = iterator;
            is_name_expr = Left is NameExpression;
        }
        public ForeachLoopStatement(BaseExpression left, BaseExpression iterator, StatementList cycleBody, SourceContext context)
            : this(left, iterator, cycleBody.Statements.ToArray(), context) { }

        private bool is_name_expr;
        private ExecutionContext last_context;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            var iterator = Iterator.GetValue(context).Cast<IEnumerable>();
            if (is_name_expr && context != last_context)
            {
                last_context = context;
                variable_index = context.LocalVariables.SetValue(Left.Cast<NameExpression>().Name, null);
            }
            var ret = default(object);
            context.StartCycle();
            foreach (var x in iterator)
            {
                if (is_name_expr) context.LocalVariables.SetValue(variable_index, x);
                else BinaryAssign.AssignToValue(context, Left, x, ref last_context, ref is_name_expr, ref variable_index);
                for (var i = 0; i < count; i++)
                {
                    ret = LoopBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
                context.Continued = false;
                if (context.Returned || context.Broken) break;
            }
            context.EndCycle();
            return ret;
        }

        public override string ToString() => $"foreach ({Left} in {Iterator}) {{{LoopBody}}}";

        public override object Clone()
        {
            return new ForeachLoopStatement(Left.CloneCast(), Iterator.CloneCast(), LoopBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
