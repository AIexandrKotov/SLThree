using Pegasus.Common;
using SLThree.Extensions.Cloning;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public class TermiteStatement : BaseStatement
    {
        public class TermiteVisitor : AbstractVisitor
        {
            public static void ReplaceNode(ref BaseStatement statement)
            {
                if (!(statement is Termited) && !(statement is FreeStatement))
                    statement = new FreeStatement(statement, statement.SourceContext);
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

        public class FreeStatement : BaseStatement
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

        public BaseStatement Statement;
        public readonly List<BaseStatement> Result = new List<BaseStatement>();
        public TermiteStatement(BaseStatement statement, SourceContext context) : base(context)
        {
            Statement = TermiteVisitor.Convert(statement);
        }


        public override string ToString() => $"Termite";
        public override object GetValue(ExecutionContext context)
        {
            Result.Clear();
            if (Statement is StatementList st)
            {
                for (var i = 0; i < st.Statements.Length; i++)
                {
                    if (st.Statements[i] is Termited)
                        Result.Add(st.Statements[i].GetValue(context) as BaseStatement);
                    else st.Statements[i].GetValue(context);
                    if (context.Broken || context.Continued) break;
                }
            }
            else
            {
                if (!context.Broken && !context.Continued)
                {
                    if (Statement is Termited)
                        Result.Add(Statement.GetValue(context) as BaseStatement);
                    else Statement.GetValue(context);
                }
            }
            return new StatementList(Result.Select(x => x.CloneCast()).ToArray(), SourceContext.CloneCast());
        }
        public override object Clone() => new TermiteStatement(Statement.CloneCast(), SourceContext.CloneCast());
    }
}
