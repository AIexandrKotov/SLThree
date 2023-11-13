﻿namespace SLThree.Visitors
{
    public interface IStatementVisitor
    {
        void VisitStatement(BaseStatement statement);

        void VisitStatement(ForeachLoopStatement statement);
        void VisitStatement(WhileLoopStatement statement);
        void VisitStatement(ExpressionStatement statement);
        void VisitStatement(ConditionStatement statement);
        void VisitStatement(ReturnStatement statement);
        void VisitStatement(SwitchStatement statement);
        void VisitStatement(UsingStatement statement);
        void VisitStatement(StatementListStatement statement);
        
        void VisitStatement(BreakStatement statement);
        void VisitStatement(ContinueStatement statement);
    }
}