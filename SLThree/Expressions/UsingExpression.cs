using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using System.Linq;

namespace SLThree
{
    public class UsingExpression : BaseExpression
    {
        public BaseExpression Alias;
        public CreatorUsing Using;
        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        public UsingExpression(BaseExpression alias, CreatorUsing usingBody, SourceContext context) : base(context)
        {
            Alias = alias;
            Using = usingBody;
        }

        public UsingExpression(CreatorUsing @using, SourceContext context) : this(null, @using, context) { }

        public override string ExpressionToString() => $"using {Using.Type}{(Alias == null ? "" : $" as {Alias}")}";

        public override object GetValue(ExecutionContext context)
        {
            var @using = Using.GetValue(context).Cast<ClassAccess>();
            string name;
            if (Alias == null)
            {
                var type_name = Using.GetTypenameWithoutGenerics();
                name = type_name.Contains(".") ? @using.Name.Name : type_name;
                name = name.Contains("`") ? name.Substring(0, name.IndexOf("`")) : name;
                variable_index = context.LocalVariables.SetValue(name, @using);
            }
            else {
                BinaryAssign.AssignToValue(context, Alias, @using, ref counted_invoked, ref is_name_expr, ref variable_index);
            }
            
            return @using;
        }

        public override object Clone()
        {
            return new UsingExpression(Alias.CloneCast(), Using.CloneCast(), SourceContext.CloneCast());
        }
    }
}
