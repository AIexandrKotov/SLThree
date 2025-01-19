using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ObjectLiteral : Literal<object>
    {
        public override int Priority => 10;
        public const string NotAvailableString = "N\\A";
        public ObjectLiteral(object value, string raw, ISourceContext context) : base(value, raw, context) { }
        public ObjectLiteral(object value, ISourceContext context) : this(value, NotAvailableString, context) { }
        public ObjectLiteral(object value) : this(value, null) { }
        public ObjectLiteral() : base() { }
        public override object Clone() => new ObjectLiteral()
        {
            Value = Value,
            SourceContext = SourceContext.CloneCast(),
            RawRepresentation = RawRepresentation
        };
    }
}
