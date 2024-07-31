namespace SLThree.Visitors
{
    public interface IConstraintVisitor
    {
        void VisitConstraint(TemplateMethod.ConstraintDefinition expression);
        void VisitConstraint(TemplateMethod.NameConstraintDefinition expression);
        void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression);
        void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression);
        void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression);
    }
}
