using SLThree.Extensions;
using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ContextStatement : BaseStatement
    {
        public CreatorContext Creator;

        public ContextStatement(CreatorContext creator, SourceContext context) : base(context)
        {
            Creator = creator;
        }

        public override string ToString() => $"context {(Creator.HasName ? Creator.Name.Name : "")} {(Creator.HasCast ? $": {Creator.Typecast}" : "")} {{\n{Creator.Body.JoinIntoString("\n")}\n}}";

        public override object GetValue(ExecutionContext context)
        {
            var created_context = Creator.GetValue(context);
            context.LocalVariables.SetValue(Creator.LastContextName, created_context);
            return created_context;
        }

        public override object Clone()
        {
            return new ContextStatement(Creator.CloneCast(), SourceContext.CloneCast());
        }
    }
}
