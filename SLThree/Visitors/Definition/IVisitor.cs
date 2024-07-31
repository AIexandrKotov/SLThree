namespace SLThree.Visitors
{
    public interface IVisitor : IExpressionVisitor, IStatementVisitor, IConstraintVisitor
    {
        void VisitAny(object o);
        void Visit(Method method);
        void Visit(GenericMethod method);
        void Visit(ExecutionContext context);
    }
}
