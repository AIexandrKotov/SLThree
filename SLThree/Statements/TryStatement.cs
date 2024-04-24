using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class TryStatement : BaseStatement
    {
        public BaseStatement[] TryBody;
        public BaseExpression CatchVariable;
        public BaseStatement[] CatchBody;
        public BaseStatement[] FinallyBody;

        public TryStatement(StatementList tryBody, BaseExpression catchVariable, StatementList catchBody, StatementList finallyBody, SourceContext context)
            : this(tryBody?.Statements.ToArray(), catchVariable, catchBody?.Statements.ToArray(), finallyBody?.Statements.ToArray(), context) { }
        public TryStatement(BaseStatement[] tryBody, BaseExpression catchVariable, BaseStatement[] catchBody, BaseStatement[] finallyBody, SourceContext context)
            : base(context)
        {
            TryBody = tryBody ?? new BaseStatement[0];
            try_count = TryBody.Length;

            CatchVariable = catchVariable;
            has_catch = CatchVariable != null;
            CatchBody = catchBody ?? new BaseStatement[0];
            catch_count = CatchBody.Length;

            FinallyBody = finallyBody ?? new BaseStatement[0];
            finally_count = FinallyBody.Length;
        }

        private int try_count;
        private bool is_name_expr;
        private ExecutionContext last_context;
        private int variable_index;
        private int catch_count;
        private bool has_catch;

        private int finally_count;

        public override string ToString() => $"try\n{{\n{TryBody}\n}}\ncatch ({CatchVariable})\n{{\n{CatchBody}\n}}\nfinally\n{{\n{FinallyBody}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = default(object);
            try
            {
                for (var i = 0; i < try_count; i++)
                {
                    ret = TryBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
            }
            catch (Exception e)
            {
                if (has_catch) BinaryAssign.AssignToValue(context, CatchVariable, e, ref last_context, ref is_name_expr, ref variable_index);
                for (var i = 0; i < catch_count; i++)
                {
                    ret = CatchBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
            }
            finally
            {
                for (var i = 0; i < finally_count; i++)
                {
                    ret = FinallyBody[i].GetValue(context);
                    if (context.Continued || context.Returned || context.Broken) break;
                }
            }
            return ret;
        }

        public override object Clone()
        {
            return new TryStatement(TryBody.CloneArray(), CatchVariable.CloneCast(), CatchBody.CloneArray(), FinallyBody.CloneArray(), SourceContext.CloneCast());
        }
    }
}
