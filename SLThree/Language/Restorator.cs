using SLThree.Extensions;
using SLThree.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Writer.WritePlainText(" ? ");
            VisitExpression(expression.Left);
            Writer.WritePlainText(" : ");
            VisitExpression(expression.Right);
        }

        public override void VisitStatement(StatementList statement)
        {
            foreach (var x in statement.Statements)
            {
                Writer.WriteTab();
                VisitStatement(x);
                Writer.Writeln();
            }
        }

        public override void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            Writer.WriteExpressionKeyword(" as ");
            VisitExpression(expression.Type);
        }

        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            if (!(statement.Expression is BlockExpression
                || statement.Expression is AccordExpression
                || statement.Expression is MatchExpression
                || statement.Expression is BaseInstanceCreator)) Writer.WritePlainText(";");
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            Writer.WriteTypeText(expression.Typename.ToString());
            if (expression.Generics != null)
            {
                Writer.WritePlainText("<");
                expression.Generics.ForeachAndBetween(VisitExpression, x => Writer.WritePlainText(", "));
                Writer.WritePlainText(">");
            }
        }

        public int CollectionLineLimit { get; set; } = -1;
        public bool CollectionCarriage { get; set; } = false;
        public override void VisitExpression(CreatorCollection expression)
        {
            Writer.WriteExpressionKeyword("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                Writer.WritePlainText("(");
                expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
                Writer.WritePlainText(")");
            }

            void CarriageBody()
            {
                Writer.WritePlainText(" {");
                Writer.Writeln();
                Writer.Level += 1;
                expression.Body.ForeachAndBetween(x =>
                {
                    Writer.WriteTab();
                    VisitExpression(x);
                }, x => Writer.WritelnPlainText(","));
                Writer.Level -= 1;
                Writer.Writeln();
                Writer.WritePlainText("}");
            }

            void LineBody()
            {
                Writer.WritePlainText(" { ");
                expression.Body.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
                Writer.WritePlainText(" }");
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
                Writer.WritePlainText("(");
                Writer.Writeln();
                Writer.Level += 1;
                expression.Expressions.ForeachAndBetween(x =>
                {
                    Writer.WriteTab();
                    VisitExpression(x);
                }, x => Writer.WritelnPlainText(","));
                Writer.Level -= 1;
                Writer.Writeln();
                Writer.WritePlainText(")");
            }
            void LineArgs()
            {
                Writer.WritePlainText("(");
                expression.Expressions.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
                Writer.WritePlainText(")");
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
                    Writer.WritePlainText(".");
                    Writer.WriteCallText(memberAccess.Right.ToString());
                    break;
                case NameExpression nameExpression:
                    Writer.WriteCallText(nameExpression.Name);
                    break;
                case Special nameExpression:
                    Writer.WriteCallText(nameExpression.ToString());
                    break;
                default:
                    VisitExpression(left);
                    break;
            }
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            GetLeftFromInvoke(expression.Left);
            Writer.WritePlainText("(");
            expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
            Writer.WritePlainText(")");
        }
        public bool AllowLineStatement { get; set; } = true;

        /// <returns>Multiline flag</returns>
        public bool OutStatement(IList<BaseStatement> statements)
        {
            if (statements.Count == 1 && AllowLineStatement)
            {
                Writer.Level += 1;
                Writer.Writeln();
                Writer.WriteTab();
                VisitStatement(statements[0]);
                Writer.Level -= 1;
                return false;
            }
            else
            {
                OutStatements(statements);
                return true;
            }
        }
        public void OutStatements(IList<BaseStatement> statements)
        {
            Writer.WritelnPlainText(" {");
            Writer.Level += 1;
            foreach (var x in statements)
            {
                Writer.WriteTab();
                VisitStatement(x);
                Writer.Writeln();
            }
            Writer.Level -= 1;
            Writer.WriteTab();
            Writer.WritePlainText("}");
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            Writer.WriteStatementKeyword("while");
            Writer.WritePlainText(" (");
            VisitExpression(statement.Condition);
            Writer.WritePlainText(")");
            OutStatement(statement.LoopBody);
        }

        public override void VisitStatement(DoWhileLoopStatement statement)
        {
            Writer.WriteStatementKeyword("do");
            var outs = OutStatement(statement.LoopBody);
            if (!outs)
            {
                Writer.Writeln();
                Writer.WriteTab();
                Writer.WriteStatementKeyword("while");
            }
            else
            {
                Writer.WriteStatementKeyword(" while");
            }
            Writer.WritePlainText(" (");
            VisitExpression(statement.Condition);
            Writer.WritePlainText(");");
        }

        public override void VisitStatement(ForeachLoopStatement statement)
        {
            Writer.WriteStatementKeyword("foreach");
            Writer.WritePlainText(" (");
            VisitExpression(statement.Left);
            Writer.WritePlainText(" in ");
            VisitExpression(statement.Iterator);
            Writer.WritePlainText(")");
            OutStatement(statement.LoopBody);
        }

        public override void VisitExpression(UsingExpression expression)
        {
            Writer.WriteStatementKeyword("using ");
            VisitExpression(expression.Using.Type);
            if (expression.Alias != null)
            {
                Writer.WriteExpressionKeyword(" as ");
                VisitExpression(expression.Alias);
            }
        }

        public bool AllowArrowFunctions { get; set; } = true;
        public bool AllowFunctionDefinitionWithoutBrackets { get; set; } = true;

        public override void VisitExpression(NameExpression expression)
        {
            if (expression.TypeHint != null)
            {
                VisitExpression(expression.TypeHint);
                Writer.WritePlainText(" ");
            }
            Writer.WritePlainText(expression.Name);
        }

        public override void VisitExpression(FunctionArgument expression)
        {
            VisitExpression(expression.Name);
            if (expression.DefaultValue != null)
            {
                Writer.WritePlainText(" = ");
                VisitExpression(expression.DefaultValue);
            }
        }

        public override void VisitExpression(FunctionDefinition expression)
        {
            expression.Modificators.ForeachAndBetween(Writer.WriteExpressionKeyword, x => Writer.WritePlainText(" "));
            if (expression.Modificators.Any()) Writer.WritePlainText(" ");
            if (expression.FunctionName != null)
                GetLeftFromInvoke(expression.FunctionName);
            if (expression.GenericArguments.Any())
            {
                Writer.WritePlainText("<");
                expression.GenericArguments.ForeachAndBetween(generic =>
                {
                    Writer.WriteTypeText(generic.Item1.Name);
                    if (generic.Item2 != null)
                    {
                        Writer.WritePlainText(": ");
                        VisitConstraint(generic.Item2);
                    }
                }, _ => Writer.WritePlainText(", "));
                Writer.WritePlainText(">");
            }


            if (AllowFunctionDefinitionWithoutBrackets && expression.FunctionName == null && expression.Arguments.Length == 1)
            {
                VisitExpression(expression.Arguments[0]);
            }
            else
            {
                Writer.WritePlainText("(");
                expression.Arguments.ForeachAndBetween(
                    VisitExpression,
                    _ => Writer.WritePlainText(", ")
                );
                Writer.WritePlainText(")");
            }

            if (expression.ReturnTypeHint != null)
            {
                Writer.WritePlainText(": ");
                VisitExpression(expression.ReturnTypeHint);
            }

            if (!expression.Modificators.Contains("abstract"))
            {
                if (AllowArrowFunctions && expression.FunctionBody.Statements.Length == 1 && expression.FunctionBody.Statements[0] is ReturnStatement)
                {
                    Writer.WritePlainText(" => ");
                    VisitExpression((expression.FunctionBody.Statements[0] as ReturnStatement).Expression);
                }
                else
                {
                    OutStatements(expression.FunctionBody.Statements);
                }
            }
        }

        public bool AllowAugmentedAssignment { get; set; } = true;
        public override void VisitExpression(BinaryOperator expression)
        {
            if (AllowAugmentedAssignment && expression is BinaryAssign assign && expression.Right is BinaryOperator bo2 && assign.Left.ToString() == bo2.Left.ToString())
            {
                VisitExpression(assign.Left);
                Writer.WritePlainText($" {bo2.Operator}= ");
                VisitExpression(bo2.Right);
            }
            else base.VisitExpression(expression);
        }
    }
}
