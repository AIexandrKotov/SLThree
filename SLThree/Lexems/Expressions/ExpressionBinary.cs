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

        public ExpressionBinary(BaseLexem left, BaseLexem right, Cursor cursor) : base(cursor)
        {
            Left = left;
            Right = right;
        }
        public ExpressionBinary() : base(default) { }

        public abstract string Operator { get; }
        public override string ToString() => $"{Left?.ToString() ?? "null"} {Operator} {Right?.ToString() ?? "null"}";
    }
}
