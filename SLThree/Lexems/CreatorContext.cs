using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class CreatorContext : BaseLexem
    {
        public NameLexem Name;
        public StatementListStatement Body;
        
        public bool HasName => Name != null;
        public bool HasBody => Body != null;

        public CreatorContext(SourceContext context) : base(context)
        {

        }

        public CreatorContext(NameLexem name, SourceContext context) : base(context)
        {
            Name = name;

        }

        public CreatorContext(NameLexem name, StatementListStatement body, SourceContext context) : base(context)
        {
            Name = name;
            Body = body;
        }

        public override string ToString() => $"context {(HasName?Name.Name:"")} {{\n{Body.Statements.JoinIntoString("\n")}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = new ExecutionContext(context);
            if (HasName) ret.Name = Name.Name;
            if (HasBody) Body.GetValue(ret);
            return new ExecutionContext.ContextWrap(ret);
        }

        public override object Clone() => new CreatorContext(Name.CloneCast(), Body.CloneCast(), SourceContext.CloneCast());
    }
}
