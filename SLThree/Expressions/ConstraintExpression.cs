using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ConstraintExpression : BaseExpression
    {
        public NameExpression Target;
        public BaseExpression Name;
        public TemplateMethod.ConstraintDefinition Body;

        private int variable_index;
        private bool is_name_expr;
        private ExecutionContext counted_invoked;

        public ConstraintExpression(NameExpression target, BaseExpression name, TemplateMethod.ConstraintDefinition left, SourceContext context) : base(context)
        {
            Name = name;
            Body = left;
            Target = target;
        }
        public ConstraintExpression(BaseExpression name, TemplateMethod.ConstraintDefinition left, SourceContext context) : base(context)
        {
            Name = name;
            Body = left;
            Target = new NameExpression("T", context.CloneCast());
        }

        public override string ExpressionToString() => $"constraint{(Target.Name == "T" ? "" : $" on {Target}")}{(Name != null ? " " + Name : "")}: {Body}";

        public override object Clone() => new ConstraintExpression(Target.CloneCast(), Name.CloneCast(), Body.CloneCast(), SourceContext.CloneCast());

        public override object GetValue(ExecutionContext context)
        {
            var constraint = Body.GetConstraint(Target.Name, context);
            if (Name != null)
                BinaryAssign.AssignToValue(context, Name, constraint, ref counted_invoked, ref is_name_expr, ref variable_index);
            return constraint;
        }
    }
}
