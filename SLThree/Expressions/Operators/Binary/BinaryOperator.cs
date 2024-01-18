using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class BinaryOperator : BaseExpression
    {
        public BaseExpression Left;
        public BaseExpression Right;

        public BinaryOperator(BaseExpression left, BaseExpression right, SourceContext context) : base(context)
        {
            Left = left;
            Right = right;
        }
        public BinaryOperator(BaseExpression left, BaseExpression right, SourceContext context, bool priority) : base(priority, context)
        {
            Left = left;
            Right = right;
        }
        public BinaryOperator() : base() { }

        public abstract string Operator { get; }
        public override string ExpressionToString() => $"{Left?.ToString() ?? "null"} {Operator} {Right?.ToString() ?? "null"}";
    }
}
