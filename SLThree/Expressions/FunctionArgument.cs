using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class FunctionArgument : BaseExpression
    {
        public NameExpression Name;
        public BaseExpression DefaultValue;

        public FunctionArgument(NameExpression name, BaseExpression value, SourceContext context) : base(context)
        {
            Name = name;
            DefaultValue = value;
        }
        public FunctionArgument(NameExpression name, SourceContext context) : this(name, null, context) { }

        public override string ExpressionToString() => $"{Name} = {DefaultValue}";

        public override object GetValue(ExecutionContext context)
        {
            throw new System.NotImplementedException();
        }

        public override object Clone()
        {
            return new FunctionArgument(Name.CloneCast(), DefaultValue.CloneCast(), SourceContext.CloneCast());
        }
    }
}
