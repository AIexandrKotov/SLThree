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
            if (!IsStatementLikeExpression(statement.Expression)) Writer.WritePlainText(";");
        }

        private static bool IsStatementLikeExpression(BaseExpression statement)
        {
            return (statement is BlockExpression
                || statement is AccordExpression
                || statement is MatchExpression
                || (statement is CreatorCollection cc && cc.Arguments.Length > 0)
                || (statement is CreatorContext creatorContext && creatorContext.CreatorBody != null)
                || statement is ConditionExpression);
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

        public const int DefaultMultiLimit = 3;
        public const bool DefaultCarriage = false;

        public int CollectionLineLimit { get; set; } = DefaultMultiLimit;
        public bool CollectionCarriage { get; set; } = DefaultCarriage;
        public override void VisitExpression(CreatorCollection expression)
        {
            Writer.WriteExpressionKeyword("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                Writer.WritePlainText("(");
                OutFunctionArguments(expression.Arguments);
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

        public int TupleLineLimit { get; set; } = DefaultMultiLimit;
        public bool TupleCarriage { get; set; } = DefaultCarriage;
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
                Writer.WritePlainText(", ");
                Writer.Level -= 1;
                Writer.Writeln();
                Writer.WritePlainText(")");
            }
            void LineArgs()
            {
                Writer.WritePlainText("(");
                expression.Expressions.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
                if (expression.Expressions.Length == 1)
                    Writer.WritePlainText(", ");
                Writer.WritePlainText(")");
            }

            if (TupleCarriage) CarriageArgs();
            else if (TupleLineLimit != -1 && expression.Expressions.Length > TupleLineLimit) CarriageArgs();
            else LineArgs();
        }

        public void WrapInBracketsIf(bool condition, Action action)
        {
            if (condition)
                Writer.WritePlainText("(");

            action();

            if (condition)
                Writer.WritePlainText(")");
        }

        public int DictionaryLineLimit { get; set; } = DefaultMultiLimit;
        public bool DictionaryCarriage { get; set; } = DefaultCarriage;
        public override void VisitExpression(CreatorDictionary expression)
        {
            void CarriageArgs()
            {
                Writer.WritePlainText("{");
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
            void LineArgs()
            {
                Writer.WritePlainText("{");
                expression.Body.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
                Writer.WritePlainText("}");
            }

            Writer.WriteExpressionKeyword("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                Writer.WritePlainText("(");
                OutFunctionArguments(expression.Arguments);
                Writer.WritePlainText(") ");
            }

            if (DictionaryCarriage) CarriageArgs();
            else if (DictionaryLineLimit != -1 && expression.Body.Length > DictionaryLineLimit) CarriageArgs();
            else LineArgs();
        }

        public override void VisitExpression(CreatorDictionary.DictionaryEntry expression)
        {
            var keyPriority = expression.Key.Priority;
            var valuePriority = expression.Value.Priority;

            WrapInBracketsIf(expression.Key.Priority > int.MinValue, () => VisitExpression(expression.Key));
            Writer.WritePlainText(": ");
            WrapInBracketsIf(expression.Value.Priority > int.MinValue, () => VisitExpression(expression.Value));
        }

        public void OutFunctionArguments(IList<BaseExpression> arguments)
        {
            void CarriageArgs()
            {
                Writer.Writeln();
                Writer.Level += 1;
                arguments.ForeachAndBetween(x =>
                {
                    Writer.WriteTab();
                    VisitExpression(x);
                }, x => Writer.WritelnPlainText(","));
                Writer.Level -= 1;
                Writer.Writeln();
            }
            void LineArgs()
            {
                arguments.ForeachAndBetween(
                    VisitExpression,
                    _ => Writer.WritePlainText(", ")
                );
            }

            if (FunctionArgumentCarriage) CarriageArgs();
            else if (FunctionArgumentLineLimit != -1 && arguments.Count > FunctionArgumentLineLimit) CarriageArgs();
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

        public override void VisitExpression(MemberAccess expression)
        {
            if (expression.Right is InvokeExpression)
            {
                VisitExpression(expression.Right as InvokeExpression, expression);
            }
            else if (expression.Right is InvokeGenericExpression)
            {
                VisitExpression(expression.Right as InvokeGenericExpression, expression);
            }
            else base.VisitExpression(expression);
        }

        /// <summary>
        /// будет писать вызовы f(x, y) в стиле x |> f(y) если y это метод
        /// </summary>
        public bool AllowFunctionalInvoke { get; set; } = true;

        private bool FunctionalInvokeCondition(MemberAccess source, BaseExpression left, BaseExpression[] arguments) => source != null;

        public override void VisitExpression(InvokeExpression expression)
        {
            VisitExpression(expression, null);
        }
        public override void VisitExpression(InvokeGenericExpression expression)
        {
            VisitExpression(expression, null);
        }

        public void VisitExpression(InvokeExpression expression, MemberAccess source = null)
        {
            var arguments = expression.Arguments;
            if (AllowFunctionalInvoke && FunctionalInvokeCondition(source, expression, arguments))
            {
                VisitExpression(arguments[0]);
                Writer.WritePlainText(" |> ");
                if (source != null)
                {
                    VisitExpression(source.Left);
                    Writer.WritePlainText(".");
                }
                arguments = arguments.Skip(1).ToArray();
            }
            else if (source != null)
            {
                VisitExpression(source.Left);
                Writer.WritePlainText(".");
            }
            GetLeftFromInvoke(expression.Left);
            Writer.WritePlainText("(");
            OutFunctionArguments(arguments);
            Writer.WritePlainText(")");
        }
        public void VisitExpression(InvokeGenericExpression expression, MemberAccess source = null)
        {
            var arguments = expression.Arguments;
            if (AllowFunctionalInvoke && FunctionalInvokeCondition(source, expression, arguments))
            {
                VisitExpression(arguments[0]);
                Writer.WritePlainText(" |> ");
                if (source != null)
                {
                    VisitExpression(source.Left);
                    Writer.WritePlainText(".");
                }
                arguments = arguments.Skip(1).ToArray();
            }
            else if (source != null)
            {
                VisitExpression(source.Left);
                Writer.WritePlainText(".");
            }
            GetLeftFromInvoke(expression.Left);
            Writer.WritePlainText("<");
            expression.GenericArguments.ForeachAndBetween(VisitExpression, x => Writer.WritePlainText(", "));
            Writer.WritePlainText(">");
            Writer.WritePlainText("(");
            OutFunctionArguments(arguments);
            Writer.WritePlainText(")");
        }

        public bool AllowLineStatement { get; set; } = true;
        public bool LineStatementOnlyForExpression { get; set; } = true;

        /// <returns>Multiline flag</returns>
        public bool OutStatement(IList<BaseStatement> statements)
        {
            if (statements.Count == 1 && AllowLineStatement && (!LineStatementOnlyForExpression || (statements[0] is ExpressionStatement exs && !IsStatementLikeExpression(exs.Expression))))
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
        public void OutStatements(IList<BaseStatement> statements, bool with_first_space = true)
        {
            Writer.WritelnPlainText(with_first_space ? " {" : "{");
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

        public override void VisitStatement(ThrowStatement statement)
        {
            Writer.WriteStatementKeyword("throw ");
            VisitExpression(statement.ThrowExpression);
            Writer.WritePlainText(";");
        }

        public override void VisitStatement(BreakStatement statement)
        {
            Writer.WriteStatementKeyword("break");
            Writer.WritePlainText(";");
        }

        public override void VisitStatement(ContinueStatement statement)
        {
            Writer.WriteStatementKeyword("continue");
            Writer.WritePlainText(";");
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            if (statement.VoidReturn)
            {
                Writer.WriteStatementKeyword("return");
            }
            else
            {
                Writer.WriteStatementKeyword("return ");
                VisitExpression(statement.Expression);
            }
            Writer.WritePlainText(";");
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

        public override void VisitExpression(NameExpression expression)
        {
            if (expression.Const)
            {
                Writer.WriteExpressionKeyword("const ");
            }
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

        public override void VisitExpression(Literal expression)
        {
            if (expression is StringLiteral stringLiteral)
            {
                var quotation = (NewLineEscaping && (stringLiteral.Value as string).Contains('\n')) ? "\"\"\"" : "\"";
                Writer.WriteStringText($"{quotation}{ReplaceEscapes(expression.Value as string, NewLineEscaping)}{quotation}");
            }
            else if (expression is AtomLiteral || expression is ByteLiteral || expression is SByteLiteral || expression is ShortLiteral || expression is UShortLiteral || expression is IntLiteral || expression is UIntLiteral || expression is LongLiteral || expression is ULongLiteral)
            {
                Writer.WriteDigitText(expression.RawRepresentation);
            }
            else base.VisitExpression(expression);
        }

        public override void VisitExpression(CreatorInstance expression)
        {
            Writer.WriteExpressionKeyword("new ");
            VisitExpression(expression.Type);
            if (expression.Arguments.Length > 0)
            {
                Writer.WritePlainText("(");
                OutFunctionArguments(expression.Arguments);
                Writer.WritePlainText(")");
            }
            if (expression.Name != null)
            {
                Writer.WritePlainText(" ");
                VisitExpression(expression.Name);
            }
            if (expression.CreatorContext != null)
            {
                if (expression.CreatorContext.Ancestors.Length > 0)
                {
                    Writer.WritePlainText(": ");
                    void CarriageArgs()
                    {
                        Writer.Writeln();
                        Writer.Level += 1;
                        expression.CreatorContext.Ancestors.ForeachAndBetween(x =>
                        {
                            Writer.WriteTab();
                            VisitExpression(x);
                        }, x => Writer.WritelnPlainText(","));
                        Writer.Level -= 1;
                        Writer.Writeln();
                    }
                    void LineArgs()
                    {
                        expression.CreatorContext.Ancestors.ForeachAndBetween(
                            VisitExpression,
                            _ => Writer.WritePlainText(", ")
                        );
                    }

                    if (AncestorCarriage) CarriageArgs();
                    else if (AncestorLineLimit != -1 && expression.CreatorContext.Ancestors.Length > AncestorLineLimit) CarriageArgs();
                    else LineArgs();
                }
                if (expression.CreatorContext.CreatorBody != null)
                    OutStatement(expression.CreatorContext.CreatorBody.Statements);
            }
        }

        public int AncestorLineLimit { get; set; } = DefaultMultiLimit;
        public bool AncestorCarriage { get; set; } = DefaultCarriage;

        public override void VisitExpression(CreatorContext expression)
        {
            if (expression.CreatorBody != null)
            {
                if (!expression.GeneratePrivate && expression.Name == null && expression.Ancestors.Length == 0)
                {
                    Writer.WriteExpressionKeyword("new");
                }
                else
                {
                    Writer.WriteExpressionKeyword("context");
                }
            }
            else
            {
                Writer.WriteExpressionKeyword("new context");
            }
            if (expression.Name != null)
            {
                Writer.WritePlainText(" ");
                VisitExpression(expression.Name);
            }
            if (expression.Ancestors.Length > 0)
            {
                Writer.WritePlainText(": ");
                void CarriageArgs()
                {
                    Writer.Writeln();
                    Writer.Level += 1;
                    expression.Ancestors.ForeachAndBetween(x =>
                    {
                        Writer.WriteTab();
                        VisitExpression(x);
                    }, x => Writer.WritelnPlainText(","));
                    Writer.Level -= 1;
                    Writer.Writeln();
                }
                void LineArgs()
                {
                    expression.Ancestors.ForeachAndBetween(
                        VisitExpression,
                        _ => Writer.WritePlainText(", ")
                    );
                }

                if (AncestorCarriage) CarriageArgs();
                else if (AncestorLineLimit != -1 && expression.Ancestors.Length > AncestorLineLimit) CarriageArgs();
                else LineArgs();
            }
            if (expression.CreatorBody != null)
            {
                OutStatements(expression.CreatorBody.Statements);
            }
        }

        public bool AllowArrowFunctions { get; set; } = true;
        public bool AllowFunctionDefinitionWithoutBrackets { get; set; } = true;
        public int FunctionArgumentLineLimit { get; set; } = DefaultMultiLimit;
        public bool FunctionArgumentCarriage { get; set; } = DefaultCarriage;
        public int FunctionGenericArgumentLineLimit { get; set; } = DefaultMultiLimit;
        public bool FunctionGenericArgumentCarriage { get; set; } = DefaultCarriage;

        public override void VisitExpression(FunctionDefinition expression)
        {
            expression.Modificators.ForeachAndBetween(Writer.WriteExpressionKeyword, x => Writer.WritePlainText(" "));
            if (expression.Modificators.Any()) Writer.WritePlainText(" ");
            if (expression.FunctionName != null)
                GetLeftFromInvoke(expression.FunctionName);
            if (expression.GenericArguments.Any())
            {
                void CarriageArgs()
                {
                    Writer.WritePlainText("<");
                    Writer.Writeln();
                    Writer.Level += 1;
                    expression.GenericArguments.ForeachAndBetween(x =>
                    {
                        Writer.WriteTab();
                        Writer.WriteTypeText(x.Item1.Name);
                        if (x.Item2 != null)
                        {
                            Writer.WritePlainText(": ");
                            VisitConstraint(x.Item2);
                        }
                    }, x => Writer.WritelnPlainText(","));
                    Writer.Level -= 1;
                    Writer.Writeln();
                    Writer.WritePlainText(">");
                }
                void LineArgs()
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

                if (FunctionArgumentCarriage) CarriageArgs();
                else if (FunctionArgumentLineLimit != -1 && expression.GenericArguments.Length > FunctionArgumentLineLimit) CarriageArgs();
                else LineArgs();


            }


            if (AllowFunctionDefinitionWithoutBrackets && expression.Modificators.Length == 0 && expression.FunctionName == null && expression.Arguments.Length == 1 && expression.FunctionBody.Statements.Length == 1 && expression.FunctionBody.Statements[0] is ReturnStatement)
            {
                VisitExpression(expression.Arguments[0]);
            }
            else
            {
                void CarriageArgs()
                {
                    Writer.WritePlainText("(");
                    Writer.Writeln();
                    Writer.Level += 1;
                    expression.Arguments.ForeachAndBetween(x =>
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
                    expression.Arguments.ForeachAndBetween(
                        VisitExpression,
                        _ => Writer.WritePlainText(", ")
                    );
                    Writer.WritePlainText(")");
                }

                if (FunctionArgumentCarriage) CarriageArgs();
                else if (FunctionArgumentLineLimit != -1 && expression.Arguments.Length > FunctionArgumentLineLimit) CarriageArgs();
                else LineArgs();
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

        public override void VisitExpression(BlockExpression expression)
        {
            OutStatements(expression.Statements, false);
        }

        public override void VisitExpression(ConditionExpression expression)
        {
            Writer.WriteStatementKeyword("if ");
            Writer.WritePlainText("(");
            VisitExpression(expression.Condition);
            Writer.WritePlainText(")");
            var multilined = OutStatement(expression.IfBody);
            if (multilined)
                Writer.WritePlainText(" ");
            var elsebody = expression.ElseBody;
            if (elsebody.Length > 0)
            {
                Writer.WriteStatementKeyword("else");
                OutStatement(elsebody);
            }
        }

        public override void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression)
        {
            VisitConstraint(expression.Left);
            Writer.WritePlainText(" | ");
            VisitConstraint(expression.Right);
        }

        public override void VisitConstraint(TemplateMethod.NameConstraintDefinition expression)
        {
            VisitExpression(expression.Name);
        }

        public override void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression)
        {
            Writer.WritePlainText("=> ");
            VisitStatement(expression.Statement);
        }

        public override void VisitConstraint(TemplateMethod.NotConstraintDefinition expression)
        {
            Writer.WritePlainText("!");
            VisitConstraint(expression.Left);
        }

        public override void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression)
        {
            VisitConstraint(expression.Left);
            Writer.WritePlainText(" + ");
            VisitConstraint(expression.Right);
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

        public bool NewLineEscaping { get; set; } = true;

        private string ReplaceEscapes(string str, bool force_to_replace_newlines, bool is_interpolated = false)
        {
            var ret = str
                .Replace("\r", "\\r")
                .Replace("\"", "\\\"")
                .Replace("\\", "\\\\")
                .Replace("\t", "\\t")
                .Replace("\n", "\\n");

            if (is_interpolated)
            {
                ret = ret.Replace("{", "{{").Replace("}", "}}");
            }

            return ret;
        }

        public override void VisitExpression(InterpolatedString expression)
        {
            var quotation = NewLineEscaping && expression.RawRepresentation.Any(x => x.Contains('\n')) ? "\"\"\"" : "\"";
            Writer.WriteStringText($"${quotation}");
            for (var i = 0; i < expression.RawRepresentation.Length; i++)
            {
                Writer.WriteStringText(ReplaceEscapes(expression.RawRepresentation[i], NewLineEscaping, true));
                if (i < expression.RawRepresentation.Length - 1)
                {
                    Writer.WritePlainText("{");
                    VisitExpression(expression.Expressions[i]);
                    Writer.WritePlainText("}");
                }
            }
            Writer.WriteStringText($"{quotation}");
        }

        public bool IsLowPriorityExpressionInMatch(BaseExpression expression)
        {
            return expression.Priority > 9;
        }

        public IList<BaseStatement> GetStatements(BaseStatement statement)
        {
            if (statement is StatementList stl)
                return stl.Statements;
            return new BaseStatement[1] { statement };
        }

        public override void VisitExpression(MatchExpression expression)
        {
            Writer.WriteStatementKeyword("match ");
            Writer.WritePlainText("(");
            VisitExpression(expression.Matching);
            Writer.WritelnPlainText(") {");
            Writer.Level += 1;

            void visitMatchExpr(BaseExpression x)
            {
                if (IsLowPriorityExpressionInMatch(x))
                {
                    Writer.WritePlainText("(");
                    VisitExpression(x);
                    Writer.WritePlainText(")");
                }
                else
                {
                    VisitExpression(x);
                }
            }

            expression.Matches.Select((x, i) => (expression.Matches[i], expression.Cases[i])).ForeachAndBetween(x =>
            {
                Writer.WriteTab();
                if (x.Item1.Length > 1)
                {
                    x.Item1.ForeachAndBetween(visitMatchExpr, _ => Writer.WritePlainText(", "));
                }
                else
                {
                    visitMatchExpr(x.Item1[0]);
                }
                if (x.Item2 is ExpressionStatement expr)
                {
                    Writer.WritePlainText(" => ");
                    VisitExpression(expr.Expression);
                }
                else
                {
                    Writer.WritePlainText(" =>");
                    OutStatement(GetStatements(x.Item2));
                }
            }, x =>
            {
                if (x.Item2 is ExpressionStatement expr)
                {
                    Writer.WritelnPlainText(";");
                }
                else
                {
                    Writer.Writeln();
                }
            });
            if (expression.Cases[expression.Cases.Length - 1] is ExpressionStatement)
                Writer.WritePlainText(";");

            if (expression.InDefault != null)
            {
                Writer.Writeln();
                Writer.WriteTab();
                if (expression.InDefault is ExpressionStatement expr)
                {
                    Writer.WritePlainText("() => ");
                    VisitExpression(expr.Expression);
                    Writer.WritePlainText(";");
                }
                else
                {
                    Writer.WritePlainText("() =>");
                    OutStatement(GetStatements(expression.InDefault));
                }
            }

            Writer.Level -= 1;
            Writer.Writeln();
            Writer.WriteTab();
            Writer.WritePlainText("}");
        }

        public override void VisitExpression(CreatorNewArray expression)
        {
            Writer.WriteExpressionKeyword("new ");
            VisitExpression(expression.ArrayType);
            Writer.WritePlainText("[");
            VisitExpression(expression.Size);
            Writer.WritePlainText("]");
        }

        public override void VisitExpression(CreatorRange expression)
        {
            if (expression.RangeType != null)
            {
                Writer.WritePlainText("<");
                VisitExpression(expression.RangeType);
                Writer.WritePlainText(">");
            }
            WrapInBracketsIf(expression.LowerBound.Priority > int.MinValue, () => VisitExpression(expression.LowerBound));
            Writer.WritePlainText(expression.Excluding ? ".." : "..=");
            WrapInBracketsIf(expression.UpperBound.Priority > int.MinValue, () => VisitExpression(expression.UpperBound));
        }

        public override void VisitExpression(CreatorUsing expression)
        {
            Writer.WriteExpressionKeyword("new using ");
            VisitExpression(expression.Type);
        }

        public override void VisitExpression(IndexExpression expression)
        {
            GetLeftFromInvoke(expression.Expression);
            Writer.WritePlainText("[");
            expression.Arguments.ForeachAndBetween(x => VisitExpression(x), x => Writer.WritePlainText(", "));
            Writer.WritePlainText("]");
        }

        public override void VisitExpression(ReferenceExpression expression)
        {
            Writer.WritePlainText("&");
            VisitExpression(expression.Expression);
        }

        public override void VisitExpression(DereferenceExpression expression)
        {
            Writer.WritePlainText("*");
            VisitExpression(expression.Expression);
        }
    }
}
