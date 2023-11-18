using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class CreatorContext : BaseLexem
    {
        public NameLexem Name;
        public BaseStatement[] Body;
        
        public bool HasName => Name != null;
        public bool HasBody => Body.Length > 0;

        public CreatorContext(SourceContext context) : base(context)
        {
            Body = new BaseStatement[0];
        }

        public CreatorContext(NameLexem name, SourceContext context) : base(context)
        {
            Name = name;
            Body = new BaseStatement[0];
        }

        public CreatorContext(BaseStatement[] body, SourceContext context) : base(context)
        {
            Body = body;
        }

        public CreatorContext(NameLexem name, BaseStatement[] body, SourceContext context) : base(context)
        {
            Name = name;
            Body = body;
        }

        public override string LexemToString() => $"context {(HasName?Name.Name:"")} {{\n{Body.JoinIntoString("\n")}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var ret = new ExecutionContext(context);
            if (HasName) ret.Name = Name.Name;
            if (HasBody)
            {
                for (var i = 0; i < Body.Length; i++)
                {
                    if (Body[i] is ExpressionStatement es && es.Lexem is ExpressionBinaryAssign assign)
                        assign.AssignValue(ret, assign.Left, assign.Right.GetValue(context));
                    else if (Body[i] is ContextStatement cs)
                        cs.GetValue(ret);
                }
            }
            return new ExecutionContext.ContextWrap(ret);
        }

        public override object Clone() => new CreatorContext(Name.CloneCast(), Body.CloneCast(), SourceContext.CloneCast());
    }
}
