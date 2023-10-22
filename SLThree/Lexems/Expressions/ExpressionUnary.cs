using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public abstract class ExpressionUnary : BaseLexem
    {
        public BaseLexem Left;
        public ExpressionUnary(BaseLexem left, Cursor cursor) : base(cursor)
        {
            Left = left;
        }
        public ExpressionUnary() : base(default) { }

        public abstract string Operator { get; }
        public override string ToString() => $"{Operator}{Left?.ToString() ?? "null"}";
    }
}
