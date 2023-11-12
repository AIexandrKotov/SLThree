using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Threading;

namespace SLThree
{
    public abstract class Literal : BaseLexem
    {
        public object Value;
        public Literal() : base() { }
        public Literal(Cursor cursor) : base(cursor) { }
        public override string ToString() => Value.ToString();
    }

    public abstract class Literal<T> : Literal
    {
        public Literal() : base() { }
        public Literal(T value, Cursor cursor) : base(cursor)
        {
            Value = value;
        }

        public override object GetValue(ExecutionContext context) => Value;
    }
}
