namespace SLThree.Visitors
{
    public interface IVisitor : IExpressionVisitor, IStatementVisitor
    {
        void VisitAny(object o);
        void Visit(Method method);
        void Visit(ExecutionContext context);
    }
}
