using SLThree.Extensions.Cloning;

namespace SLThree
{
    public class ConstraintExpression : BaseExpression
    {
        public bool DefaultTarget;
        public NameExpression Target;
        public BaseExpression Name;
        public TemplateMethod.ConstraintDefinition Body;

        private int variable_index;
        private bool is_name_expr;
        private ExecutionContext counted_invoked;

        public ConstraintExpression(NameExpression target, BaseExpression name, TemplateMethod.ConstraintDefinition left, ISourceContext context) : base(context)
        {
            Name = name;
            Body = left;
            Target = target;
            DefaultTarget = false;
        }
        public ConstraintExpression(BaseExpression name, TemplateMethod.ConstraintDefinition left, ISourceContext context) : base(context)
        {
            Name = name;
            Body = left;
            DefaultTarget = true;
        }

        public override string ExpressionToString() => $"constraint{(DefaultTarget ? "" : $" on {Target}")}{(Name != null ? " " + Name : "")}: {Body}";

        public override object Clone() =>
            DefaultTarget ? new ConstraintExpression(Name.CloneCast(), Body.CloneCast(), SourceContext.CloneCast())
            : new ConstraintExpression(Target.CloneCast(), Name.CloneCast(), Body.CloneCast(), SourceContext.CloneCast());

        public override object GetValue(ExecutionContext context)
        {
            var constraint = Body.GetConstraint(DefaultTarget ? "T" : Target.Name, context);
            if (Name != null)
                BinaryAssign.AssignToValue(context, Name, constraint, ref counted_invoked, ref is_name_expr, ref variable_index);
            return constraint;
        }
    }
}
