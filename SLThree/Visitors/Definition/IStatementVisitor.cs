﻿namespace SLThree.Visitors
{
    public interface IStatementVisitor
    {
        void VisitStatement(BaseStatement statement);

        void VisitStatement(BreakStatement statement);
        void VisitStatement(ContinueStatement statement);
        void VisitStatement(ExpressionStatement statement);
        void VisitStatement(ForeachLoopStatement statement);
        void VisitStatement(ReturnStatement statement);
        void VisitStatement(StatementList statement);
        void VisitStatement(ThrowStatement statement);
        void VisitStatement(TryStatement statement);
        void VisitStatement(WhileLoopStatement statement);
    }
}
