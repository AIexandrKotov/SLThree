using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class ExpressionUnary : BaseExpression
    {
        public BaseExpression Left;
        public ExpressionUnary(BaseExpression left, SourceContext context) : base(context)
        {
            Left = left;
        }
        public ExpressionUnary(BaseExpression left, SourceContext context, bool priority) : base(priority, context)
        {
            Left = left;
        }
        public ExpressionUnary() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString() => $"{Operator}{Left?.ToString() ?? "null"}";
    }
}
