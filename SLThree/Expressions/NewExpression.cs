using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;
using System.Linq;

namespace SLThree
{
    public class NewExpression : BaseExpression
    {
        public InvokeExpression InvokeExpression;
        public MemberAccess MemberAccess;

        public NewExpression() : base() { }

        public NewExpression(MemberAccess memberAccess, Cursor cursor) : base(cursor)
        {
            MemberAccess = memberAccess;
        }

        public NewExpression(InvokeExpression invokeExpression, Cursor cursor) : base(cursor)
        {
            InvokeExpression = invokeExpression;
        }

        public override string ExpressionToString() => $"new {MemberAccess}";

        private bool counted_name;
        private string invk_name;
        public override object GetValue(ExecutionContext context)
        {
            if (MemberAccess != null) return MemberAccess.Create(context);
            else
            {
                if (!counted_name)
                {
                    invk_name = InvokeExpression.Left.ExpressionToString();
                    counted_name = true;
                }
                return Activator.CreateInstance(
                context.LocalVariables.GetValue(invk_name).Item1.Cast<MemberAccess.ClassAccess>().Name,
                InvokeExpression.Arguments.Select(x => x.GetValue(context)).ToArray());
            }
        }

        public override object Clone() => new NewExpression()
        {
            MemberAccess = MemberAccess.CloneCast(),
            InvokeExpression = InvokeExpression.CloneCast(),
            SourceContext = SourceContext.CloneCast(),
        };
    }
}
