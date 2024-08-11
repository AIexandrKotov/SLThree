using Pegasus.Common;
using SLThree.Extensions.Cloning;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using static SLThree.TermiteList;

namespace SLThree
{
    /// <summary>
    /// Базовый класс всех термитов
    /// </summary>
    public abstract class BaseTermite : BaseStatement
    {
        public BaseTermite()
        {
        }

        public BaseTermite(SourceContext context) : base(context)
        {
        }

        public static void Apply(List<BaseStatement> result, BaseStatement source, ExecutionContext context)
        {
            if (source is StatementList st)
            {
                for (var i = 0; i < st.Statements.Length; i++)
                {
                    if (st.Statements[i] is Termited)
                        result.Add(st.Statements[i].GetValue(context) as BaseStatement);
                    else st.Statements[i].GetValue(context);
                    if (context.Broken || context.Continued) break;
                }
            }
            else
            {
                if (!context.Broken && !context.Continued)
                {
                    if (source is Termited)
                        result.Add(source.GetValue(context) as BaseStatement);
                    else source.GetValue(context);
                }
            }
        }
        public static BaseStatement ResultCollect(List<BaseStatement> result)
        {
            var res = result.SelectMany(x => x is BaseTermite termite ? new BaseStatement[1] { termite.Collect() } : Enumerable.Empty<BaseStatement>()).ToArray();
            if (res.Length == 1) return res[0];
            else return new StatementList(res, res.FirstOrDefault()?.SourceContext.CloneCast());
        }
        public static StatementList ResultCollectList(List<BaseStatement> result)
        {
            var res = result.SelectMany(x => x is BaseTermite termite ? new BaseStatement[1] { termite.Collect() } : Enumerable.Empty<BaseStatement>()).ToArray();
            return new StatementList(res, res.FirstOrDefault()?.SourceContext.CloneCast());
        }

        public abstract BaseStatement Collect();
    }

    public class TermiteVisitor : AbstractVisitor
    {
        public static void ReplaceNode(ref BaseStatement statement)
        {
            if (statement is Termited) 
                return;
            if (statement is BaseTermite)
                return;
            switch (statement)
            {
                case WhileLoopStatement whileLoopStatement:
                    statement = new TermiteWhile(whileLoopStatement, new StatementList(whileLoopStatement.LoopBody, whileLoopStatement.SourceContext), whileLoopStatement.SourceContext);
                    break;
            }
            if (!(statement is Termited) && !(statement is BaseTermite))
            {
                statement = new FreeStatement(statement, statement.SourceContext);
            }
        }

        public override void VisitStatement(StatementList statement)
        {
            for (var i = 0; i < statement.Statements.Length; i++)
                ReplaceNode(ref statement.Statements[i]);
            base.VisitStatement(statement);
        }

        public override void VisitStatement(ForeachLoopStatement statement)
        {
            for (var i = 0; i < statement.LoopBody.Length; i++)
                ReplaceNode(ref statement.LoopBody[i]);
            base.VisitStatement(statement);
        }

        public override void VisitStatement(TryStatement statement)
        {
            for (var i = 0; i < statement.TryBody.Length; i++)
                ReplaceNode(ref statement.TryBody[i]);
            for (var i = 0; i < statement.CatchBody.Length; i++)
                ReplaceNode(ref statement.CatchBody[i]);
            for (var i = 0; i < statement.FinallyBody.Length; i++)
                ReplaceNode(ref statement.FinallyBody[i]);
            base.VisitStatement(statement);
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            for (var i = 0; i < statement.LoopBody.Length; i++)
                ReplaceNode(ref statement.LoopBody[i]);
            base.VisitStatement(statement);
        }

        public override void VisitExpression(BlockExpression expression)
        {
            for (var i = 0; i < expression.Statements.Length; i++)
                ReplaceNode(ref expression.Statements[i]);
            base.VisitExpression(expression);
        }

        public override void VisitStatement(BaseStatement statement)
        {
            switch (statement)
            {
                case Termited termited:
                    base.VisitStatement(termited.Statement);
                    break;
                default:
                    base.VisitStatement(statement);
                    break;
            }

        }

        public static BaseStatement Convert(BaseStatement statement)
        {
            var tv = new TermiteVisitor();
            tv.VisitStatement(statement);
            return statement;
        }
    }
    public class FreeStatement : BaseTermite
    {
        public BaseStatement Statement;

        public FreeStatement(BaseStatement statement, SourceContext context) : base(context)
        {
            Statement = statement;
        }

        public override string ToString() => Statement.ToString();

        public override object GetValue(ExecutionContext context)
        {
            return Statement;
        }

        public override object Clone() => new Termited(Statement.CloneCast(), SourceContext.CloneCast());

        public override BaseStatement Collect() => Statement;
    }
    public class Termited : BaseStatement
    {
        public BaseStatement Statement;

        public Termited(BaseStatement statement, SourceContext context) : base(context)
        {
            Statement = statement;
        }

        public override string ToString() => $"#{Statement}";

        public override object GetValue(ExecutionContext context)
        {
            if (Statement is Termited termited) return termited.GetValue(context) as StatementList;
            return Statement.GetValue(context);
        }

        public override object Clone() => new Termited(Statement.CloneCast(), SourceContext.CloneCast());
    }
    public class TermiteList : BaseTermite
    {
        public BaseStatement Statement;
        public readonly List<BaseStatement> Result = new List<BaseStatement>();
        public TermiteList(BaseStatement statement, SourceContext context) : base(context)
        {
            Statement = TermiteVisitor.Convert(statement);
        }


        public override string ToString() => $"Termite";
        public override object GetValue(ExecutionContext context)
        {
            Apply(Result, Statement, context);
            return Result;
        }
        public override BaseStatement Collect()
        {
            return ResultCollect(Result);
        }

        public override object Clone() => new TermiteList(Statement.CloneCast(), SourceContext.CloneCast());
    }
    public class TermiteWhile : BaseTermite
    {
        public WhileLoopStatement Head;
        public BaseStatement Statement;
        public readonly List<BaseStatement> Result = new List<BaseStatement>();
        public TermiteWhile(WhileLoopStatement whileLoopStatement, BaseStatement statement, SourceContext context) : base(context)
        {
            Head = whileLoopStatement;
            Statement = TermiteVisitor.Convert(statement);
        }

        public override string ToString() => $"Termite";
        public override object GetValue(ExecutionContext context)
        {
            Apply(Result, Statement, context);
            return Result;
        }
        public override BaseStatement Collect()
        {
            return new WhileLoopStatement(Head.Condition, ResultCollectList(Result), Head.SourceContext);
        }

        public override object Clone() => new TermiteWhile(Head.CloneCast(), Statement.CloneCast(), SourceContext.CloneCast());
    }
    public class TermiteCondition : BaseTermite
    {
        public BaseExpression Condition;
        public BaseStatement TrueStatement;
        public BaseStatement FalseStatement;
        public readonly List<BaseStatement> TrueResult = new List<BaseStatement>();
        public readonly List<BaseStatement> FalseResult = new List<BaseStatement>();
        public TermiteCondition(BaseStatement statement, SourceContext context) : base(context)
        {
            TrueStatement = TermiteVisitor.Convert(statement);
        }

        public override string ToString() => $"Termite";
        public override object GetValue(ExecutionContext context)
        {
            Apply(TrueResult, TrueStatement, context);
            Apply(FalseResult, FalseStatement, context);
            return (TrueResult, FalseResult);
        }
        public override BaseStatement Collect()
        {
            return new ExpressionStatement(new ConditionExpression(Condition, ResultCollectList(TrueResult), ResultCollectList(FalseResult), SourceContext.CloneCast()), SourceContext.CloneCast());
        }

        public override object Clone() => new TermiteCondition(TrueStatement.CloneCast(), SourceContext.CloneCast());
    }
}
