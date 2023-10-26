using Pegasus.Common;
using System.Threading;

namespace SLThree
{
    public abstract class Literal<T> : BaseLexem
    {
        public T Value;
        public Literal() : this(default, default) { }
        public Literal(T value, Cursor cursor) : base(cursor)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
        //public abstract SLTSpeedyObject GetValue(ExecutionContext context);
    }

    public abstract class BoxLiteral<T> : BoxSupportedLexem
    {
        public T Value;
        public BoxLiteral() : this(default, default) { }
        public BoxLiteral(T value, Cursor cursor) : base(cursor)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}
