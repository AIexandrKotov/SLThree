using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
            WritePlainText(" ? ");
            VisitExpression(expression.Left);
            WritePlainText(" : ");
            VisitExpression(expression.Right);
        }

        public override void VisitStatement(StatementList statement)
        {
            foreach (var x in statement.Statements)
            {
                WriteTab();
                VisitStatement(x);
                Writeln();
            }
        }

        public override void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            WriteExpressionKeyword(" as ");
            VisitExpression(expression.Type);
        }

        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            if (!(statement.Expression is BlockExpression
                || statement.Expression is AccordExpression
                || statement.Expression is MatchExpression
                || statement.Expression is BaseInstanceCreator)) WritePlainText(";");
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            WriteTypeText(expression.Typename.ToString());
            if (expression.Generics != null)
            {
                WritePlainText("<");
                expression.Generics.ForeachAndBetween(VisitExpression, x => WritePlainText(", "));
                WritePlainText(">");
            }
        }

        public int CollectionLineLimit { get; set; } = -1;
        public bool CollectionCarriage { get; set; } = false;
        public override void VisitExpression(CreatorCollection expression)
        {
            WriteExpressionKeyword("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                WritePlainText("(");
                expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => WritePlainText(", "));
                WritePlainText(")");
            }

            void CarriageBody()
            {
                WritePlainText(" {");
                Writeln();
                Level += 1;
                expression.Body.ForeachAndBetween(x =>
                {
                    WriteTab();
                    VisitExpression(x);
                }, x => WritelnPlainText(","));
                Level -= 1;
                Writeln();
                WritePlainText("}");
            }

            void LineBody()
            {
                WritePlainText(" { ");
                expression.Body.ForeachAndBetween(x => VisitExpression(x), x => WritePlainText(", "));
                WritePlainText(" }");
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
                WritePlainText("(");
                Writeln();
                Level += 1;
                expression.Expressions.ForeachAndBetween(x =>
                {
                    WriteTab();
                    VisitExpression(x);
                }, x => WritelnPlainText(","));
                Level -= 1;
                Writeln();
                WritePlainText(")");
            }
            void LineArgs()
            {
                WritePlainText("(");
                expression.Expressions.ForeachAndBetween(x => VisitExpression(x), x => WritePlainText(", "));
                WritePlainText(")");
            }

            if (CollectionCarriage) CarriageArgs();
            else if (CollectionLineLimit != -1 && expression.Expressions.Length > CollectionLineLimit) CarriageArgs();
            else LineArgs();
        }

        private void GetLeftFromInvoke(BaseExpression left)
        {
            switch (left)
            {
                case MemberAccess memberAccess:
                    VisitExpression(memberAccess.Left);
                    WritePlainText(".");
                    WriteCallText(memberAccess.Right.ToString());
                    break;
                case NameExpression nameExpression:
                    WriteCallText(nameExpression.Name);
                    break;
                case Special nameExpression:
                    WriteCallText(nameExpression.ToString());
                    break;
                default:
                    VisitExpression(left);
                    break;
            }
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            GetLeftFromInvoke(expression.Left);
            WritePlainText("(");
            expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => WritePlainText(", "));
            WritePlainText(")");
        }
        public bool AllowLineStatement { get; set; } = true;

        public void OutStatement(IList<BaseStatement> statements)
        {
            if (statements.Count == 1 && AllowLineStatement)
            {
                Level += 1;
                Writeln();
                WriteTab();
                VisitStatement(statements[0]);
                Level -= 1;
            }
            else OutStatements(statements);
        }
        public void OutStatements(IList<BaseStatement> statements)
        {
            WritelnPlainText(" {");
            Level += 1;
            foreach (var x in statements)
            {
                WriteTab();
                VisitStatement(x);
                Writeln();
            }
            Level -= 1;
            WriteTab();
            WritePlainText("}");
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            WriteStatementKeyword("while");
            WritePlainText(" (");
            VisitExpression(statement.Condition);
            WritePlainText(")");
            OutStatement(statement.LoopBody);
        }

        public override void VisitExpression(UsingExpression expression)
        {
            WriteStatementKeyword("using ");
            VisitExpression(expression.Using.Type);
            if (expression.Alias != null)
            {
                WriteExpressionKeyword(" as ");
                VisitExpression(expression.Alias);
            }
        }
    }
}
