using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Threading;

namespace SLThree
{
    public abstract class Literal : BaseLexem
    {
        public object Value;
        public string RawRepresentation = "";
        public Literal() : base() { }
        public Literal(SourceContext context) : base(context) { }
        public override string LexemToString() => RawRepresentation;
    }

    public abstract class Literal<T> : Literal
    {
        public Literal() : base() { }
        public Literal(T value, SourceContext context) : base(context)
        {
            Value = value;
            RawRepresentation = Value.ToString();
        }
        public Literal(T value, string raw, SourceContext context) : base(context)
        {
            Value = value;
            RawRepresentation = raw;
        }

        public override object GetValue(ExecutionContext context) => Value;
    }
}
