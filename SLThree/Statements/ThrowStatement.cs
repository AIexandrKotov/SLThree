using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System;

namespace SLThree
{
    public class ThrowStatement : BaseStatement
    {
        public BaseExpression ThrowExpression;
        public ThrowStatement(BaseExpression expression, SourceContext context) : base(context)
        {
            ThrowExpression = expression;
        }

        public override string ToString() => $"throw {ThrowExpression}";
        public override object GetValue(ExecutionContext context)
        {
            throw ThrowExpression.GetValue(context).Cast<Exception>();
        }
        public override object Clone() => new ThrowStatement(ThrowExpression.CloneCast(), SourceContext.CloneCast());
    }
}
