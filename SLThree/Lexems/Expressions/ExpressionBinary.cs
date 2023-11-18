using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class ExpressionBinary : BaseLexem
    {
        public BaseLexem Left;
        public BaseLexem Right;

        public ExpressionBinary(BaseLexem left, BaseLexem right, SourceContext context) : base(context)
        {
            Left = left;
            Right = right;
        }
        public ExpressionBinary(BaseLexem left, BaseLexem right, SourceContext context, bool priority) : base(priority, context)
        {
            Left = left;
            Right = right;
        }
        public ExpressionBinary() : base() { }

        public abstract string Operator { get; }
        public override string LexemToString() => $"{Left?.ToString() ?? "null"} {Operator} {Right?.ToString() ?? "null"}";
    }
}
