using Pegasus.Common;
using SLThree.Extensions.Cloning;
using System.Threading;

namespace SLThree
{
    public abstract class Literal : BaseExpression
    {
        public object Value;
        public Literal() : base() { }
        public Literal(SourceContext context) : base(context) { }

        private string rawRepresentation = "";
        public string RawRepresentation { 
            get => string.IsNullOrEmpty(rawRepresentation) ? Value.ToString() : rawRepresentation;
            set => rawRepresentation = value; 
        }

        public override string ExpressionToString() => RawRepresentation;
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
