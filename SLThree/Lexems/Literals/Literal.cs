using Pegasus.Common;
using System.Threading;

namespace SLThree
{
    public partial class Literal<T> : BaseLexem
    {
        public object Value;
        public Literal() : this(default, default) { }
        public Literal(T value, Cursor cursor) : base(cursor)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        public override object GetValue(ExecutionContext context) => Value;
    }
}
