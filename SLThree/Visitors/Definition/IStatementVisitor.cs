namespace SLThree.Visitors
{
    public interface IStatementVisitor
    {
        void VisitStatement(BaseStatement statement);

        void VisitStatement(BreakStatement statement);
        void VisitStatement(ConditionStatement statement);
        void VisitStatement(ContinueStatement statement);
        void VisitStatement(ExpressionStatement statement);
        void VisitStatement(ForeachLoopStatement statement);
        void VisitStatement(ReturnStatement statement);
        void VisitStatement(StatementListStatement statement);
        void VisitStatement(SwitchStatement statement);
        void VisitStatement(ThrowStatement statement);
        void VisitStatement(TryStatement statement);
        void VisitStatement(UsingStatement statement);
        void VisitStatement(WhileLoopStatement statement);
    }
}
