using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;

namespace SLThree.Language
{
    public class Restorator : DefaultRestorator, IRestorator
    {
        public override string LanguageName => "SLThree";
        public string Restore(BaseStatement statement, ExecutionContext context) => Restore<Restorator>(statement, context);
        public string Restore(BaseExpression expression, ExecutionContext context) => Restore<Restorator>(expression, context);

        public override void VisitExpression(TernaryOperator expression)
        {
            VisitExpression(expression.Condition);
            Write(" ? ");
            VisitExpression(expression.Left);
            Write(" : ");
            VisitExpression(expression.Right);
        }

        public override void VisitStatement(StatementList statement)
        {
            foreach (var x in statement.Statements)
            {
                WriteTab();
                VisitStatement(x);
                Writeln("");
            }
        }

        public override void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            Write(" as ");
            VisitExpression(expression.Type);
        }

        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            if (!(statement.Expression is BlockExpression
                || statement.Expression is AccordExpression
                || statement.Expression is MatchExpression
                || statement.Expression is BaseInstanceCreator)) Write(";");
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            VisitExpression(expression.Typename);
            if (expression.Generics != null)
            {
                Write("<");
                expression.Generics.ForeachAndBetween(x => VisitExpression(x), x => Write(", "));
                Write(">");
            }
        }

        public int CollectionLineLimit { get; set; } = -1;
        public bool CollectionCarriage { get; set; } = false;
        public override void VisitExpression(CreatorCollection expression)
        {
            Write("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                Write("(");
                expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => Write(", "));
                Write(")");
            }

            void CarriageBody()
            {
                Write(" {");
                Writeln("");
                Level += 1;
                expression.Body.ForeachAndBetween(x =>
                {
                    WriteTab();
                    VisitExpression(x);
                }, x => Writeln(","));
                Level -= 1;
                Writeln("");
                Write("}");
            }

            void LineBody()
            {
                Write(" { ");
                expression.Body.ForeachAndBetween(x => VisitExpression(x), x => Write(", "));
                Write(" }");
            }

            if (CollectionCarriage) CarriageBody();
            else if (CollectionLineLimit != -1 && expression.Body.Length > CollectionLineLimit) CarriageBody();
            else LineBody();
        }

        public int TupleLineLimit { get; set; } = -1;
        public bool TupleCarriage { get; set; } = false;
        public override void VisitExpression(CreatorTuple expression)
        {
            void CarriageArgs()
            {
                Write("(");
                Writeln("");
                Level += 1;
                expression.Expressions.ForeachAndBetween(x =>
                {
                    WriteTab();
                    VisitExpression(x);
                }, x => Writeln(","));
                Level -= 1;
                Writeln("");
                Write(")");
            }
            void LineArgs()
            {
                Write("(");
                expression.Expressions.ForeachAndBetween(x => VisitExpression(x), x => Write(", "));
                Write(")");
            }

            if (CollectionCarriage) CarriageArgs();
            else if (CollectionLineLimit != -1 && expression.Expressions.Length > CollectionLineLimit) CarriageArgs();
            else LineArgs();
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            VisitExpression(expression.Left);
            Write("(");
            expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => Write(", "));
            Write(")");
        }
        public bool AllowLineStatement { get; set; } = true;

        public void OutStatement(IList<BaseStatement> statements)
        {
            if (statements.Count == 1 && AllowLineStatement)
            {
                Level += 1;
                Writeln("");
                WriteTab();
                VisitStatement(statements[0]);
                Level -= 1;
            }
            else OutStatements(statements);
        }
        public void OutStatements(IList<BaseStatement> statements)
        {
            Writeln(" {");
            Level += 1;
            foreach (var x in statements)
            {
                WriteTab();
                VisitStatement(x);
                Writeln("");
            }
            Level -= 1;
            WriteTab();
            Write("}");
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            Write("while (");
            VisitExpression(statement.Condition);
            Write(")");
            OutStatement(statement.LoopBody);
        }
    }
}
