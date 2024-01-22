using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Net.NetworkInformation;

namespace SLThree
{
    public class ReturnStatement : BaseStatement
    {
        public bool VoidReturn;
        public BaseExpression Expression;

        public ReturnStatement() : base() { }
        public ReturnStatement(BaseExpression expression, SourceContext context) : base(context)
        {
            Expression = expression;
        }

        public ReturnStatement(SourceContext context) : base(context)
        {
            VoidReturn = true;
        }

        public override string ToString() => $"{Expression}";
        public override object GetValue(ExecutionContext context)
        {
            if (VoidReturn)
            {
                context.Return();
                return null;
            }
            else
            {
                var value = Expression.GetValue(context);
                context.Return(value);
                return value;
            }
        }

        public override object Clone()
        {
            return new ReturnStatement() { Expression = Expression.CloneCast(), VoidReturn = VoidReturn.Copy(), SourceContext = SourceContext.CloneCast() };
        }
    }
}
