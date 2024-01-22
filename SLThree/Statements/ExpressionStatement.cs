using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public class ExpressionStatement : BaseStatement
    {
        public BaseExpression Expression;

        public ExpressionStatement(BaseExpression expression, SourceContext context) : base(context)
        {
            Expression = expression;
        }

        public override string ToString() => $"{Expression}";
        public override object GetValue(ExecutionContext context) => Expression.GetValue(context);

        public override object Clone()
        {
            return new ExpressionStatement(Expression.CloneCast(), SourceContext.CloneCast());
        }
    }
}
