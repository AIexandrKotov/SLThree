using SLThree.Extensions.Cloning;

namespace SLThree
{

    public class NameExpression : BaseExpression
    {
        public TypenameExpression TypeHint;
        public string Name;
        public NameExpression() : base() { }
        public NameExpression(string name, ISourceContext context) : this(name, null, context) { }
        public NameExpression(string name, TypenameExpression typehint, ISourceContext context) : base(context)
        {
            Name = name;
            TypeHint = typehint;
        }

        public override string ExpressionToString() => $"{(TypeHint == null?"":TypeHint.ToString())}{Name}";

        private ExecutionContext counted;
        private int variable_index;
        public override object GetValue(ExecutionContext context)
        {
            if (counted == context)
            {
                return context.LocalVariables.GetValue(variable_index);
            }
            else
            {
                var (value, ind) = context.LocalVariables.GetValue(Name);
                if (ind == -1) return value;
                counted = context;
                variable_index = ind;
                return value;
            }
        }

        public NameExpression Hint(TypenameExpression typehint)
        {
            TypeHint = typehint;
            return this;
        }

        public override object Clone()
        {
            return new NameExpression(Name.CloneCast(), TypeHint.CloneCast(), SourceContext = SourceContext.CloneCast());
        }
    }
}
