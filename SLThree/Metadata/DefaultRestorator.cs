using SLThree.Visitors;
using System.Linq.Expressions;
using System.Text;
using System;
using System.Collections.Generic;

namespace SLThree.Metadata
{
    public class DefaultRestorator : AbstractVisitor
    {
        public interface IRestoratorWriter
        {
            int Tabulation { get; set; }
            int Level { get; set; }

            void RemoveEndNewlines();
            string Unsupported(object o);
            void WriteCallText(string s);
            void WriteExpressionKeyword(string s);
            void Writeln();
            void WritelnPlainText(string s);
            void WritePlainText(string s);
            void WriteStatementKeyword(string s);
            void WriteStringText(string s);
            void WriteTab();
            void WriteTypeText(string s);
            void WriteErrorText(string s);
            void Clear();
            string GetString();
            void WriteDigitText(string s);
        }
        public class DefaultRestoratorWriter : IRestoratorWriter
        {
            public virtual int Tabulation { get; set; } = 4;
            public virtual void WriteTab() => Sb.Append(new string(' ', Tabulation * Level));
            public virtual void WritePlainText(string s) => Sb.Append(s);
            public virtual void WriteTypeText(string s) => Sb.Append(s);
            public virtual void WriteCallText(string s) => Sb.Append(s);
            public virtual void WriteExpressionKeyword(string s) => Sb.Append(s);
            public virtual void WriteStatementKeyword(string s) => Sb.Append(s);
            public virtual void WriteStringText(string s) => Sb.Append(s);
            public virtual void WriteErrorText(string s) => Sb.Append(s);
            public virtual void WriteDigitText(string s) => Sb.Append(s);
            public virtual void WritelnPlainText(string s) => Sb.AppendLine(s);
            public virtual void Writeln() => Sb.AppendLine();
            public virtual string Unsupported(object o) => $"\"!{o.GetType().FullName}!\"";
            public void RemoveEndNewlines()
            {
                if (Sb.Length == 0) return;
                while (Sb[Sb.Length - 1] == '\n' || Sb[Sb.Length - 1] == '\r')
                    Sb.Remove(Sb.Length - 1, 1);
            }

            public StringBuilder Sb { get; set; } = new StringBuilder();
            public int Level { get; set; } = 0;

            public void Clear()
            {
                Sb.Clear();
            }

            public string GetString()
            {
                return Sb.ToString();
            }
        }
        public class ConsoleWriter : IRestoratorWriter
        {
            public virtual int Tabulation { get; set; } = 4;
            public virtual void WriteTab() => Console.Write(new string(' ', Tabulation * Level));
            public ConsoleColor BackColor { get; set; } = ConsoleColor.White;
            public Dictionary<string, ConsoleColor> Colors { get; set; } = new Dictionary<string, ConsoleColor>()
            {
                { "Plain", ConsoleColor.Black },
                { "Type", ConsoleColor.DarkCyan },
                { "Call", ConsoleColor.DarkYellow },
                { "ExpressionKeyword", ConsoleColor.Blue },
                { "StatementKeyword", ConsoleColor.DarkMagenta },
                { "String", ConsoleColor.DarkRed },
                { "Error", ConsoleColor.Red },
                { "Digit", ConsoleColor.DarkGreen },
            };
            public virtual void WritePlainText(string s)
            {
                Console.ForegroundColor = Colors["Plain"];
                Console.Write(s.Replace("\r", ""));
            }
            public virtual void WriteTypeText(string s)
            {
                Console.ForegroundColor = Colors["Type"];
                Console.Write(s);
            }
            public virtual void WriteCallText(string s)
            {
                Console.ForegroundColor = Colors["Call"];
                Console.Write(s);
            }
            public virtual void WriteExpressionKeyword(string s)
            {
                Console.ForegroundColor = Colors["ExpressionKeyword"];
                Console.Write(s);
            }
            public virtual void WriteStatementKeyword(string s)
            {
                Console.ForegroundColor = Colors["StatementKeyword"];
                Console.Write(s);
            }
            public virtual void WritelnPlainText(string s)
            {
                Console.ForegroundColor = Colors["Plain"];
                Console.Write(s + "\n");
            }
            public virtual void WriteStringText(string s)
            {
                Console.ForegroundColor = Colors["String"];
                Console.Write(s);
            }
            public virtual void WriteErrorText(string s)
            {
                Console.ForegroundColor = Colors["Error"];
                Console.Write(s);
            }
            public virtual void WriteDigitText(string s)
            {
                Console.ForegroundColor = Colors["Digit"];
                Console.Write(s);
            }

            public virtual void Writeln() => Console.WriteLine();
            public virtual string Unsupported(object o) => $"\"!{o.GetType().FullName}!\"";

            public int Level { get; set; } = 0;

            public void RemoveEndNewlines()
            {
                return;
            }
            public void Clear()
            {
                Console.BackgroundColor = BackColor;
                return;
            }

            public string GetString()
            {
                return "ConsoleWriter doesn't support str-out";
            }
        }

        public IRestoratorWriter Writer { get; set; } = new DefaultRestoratorWriter();
        public virtual string LanguageName { get; } = "LANGNAME";

        public static T GetRestorator<T>(ExecutionContext context = null) where T: DefaultRestorator, new()
        {
            return context?.Unwrap<T>() ?? new T();
        }

        public static string Restore<T>(BaseStatement statement, ExecutionContext context) where T: DefaultRestorator, new()
        {
            var restorator = GetRestorator<T>(context);
            return restorator.Restore(statement);
        }
        public static string Restore<T>(BaseExpression expression, ExecutionContext context) where T : DefaultRestorator, new()
        {
            var restorator = GetRestorator<T>(context);
            return restorator.Restore(expression);
        }
        public string Restore(BaseStatement statement)
        {
            Writer.Level = 0;
            Writer.Clear();
            VisitStatement(statement);
            Writer.RemoveEndNewlines();
            return Writer.GetString();
        }
        public string Restore(BaseExpression expression)
        {
            Writer.Level = 0;
            Writer.Clear();
            VisitExpression(expression);
            Writer.RemoveEndNewlines();
            return Writer.GetString();
        }


        public override void VisitStatement(StatementList statement)
        {
            Writer.Level += 1;
            Writer.WritelnPlainText(Writer.Unsupported(statement));
            Writer.Level -= 1;
        }
        public override void VisitExpression(Literal expression)
        {
            if (expression is NullLiteral || expression is BoolLiteral)
            {
                Writer.WriteExpressionKeyword(expression.RawRepresentation);
            }
            else Writer.WritePlainText(expression.RawRepresentation);
        }
        public override void VisitExpression(UnaryOperator expression)
        {
            Writer.WritePlainText($"{expression.Operator}");
            if (expression.Left.Priority > expression.Priority)
            {
                Writer.WritePlainText("(");
            }
            VisitExpression(expression.Left);
            if (expression.Left.Priority > expression.Priority)
            {
                Writer.WritePlainText(")");
            }
        }
        public override void VisitExpression(BinaryOperator expression)
        {
            if (expression.Left.Priority > expression.Priority)
            {
                Writer.WritePlainText("(");
            }
            VisitExpression(expression.Left);
            if (expression.Left.Priority > expression.Priority)
            {
                Writer.WritePlainText(")");
            }
            Writer.WritePlainText($" {expression.Operator} ");
            if (expression.Right.Priority > expression.Priority)
            {
                Writer.WritePlainText("(");
            }
            VisitExpression(expression.Right);
            if (expression.Right.Priority > expression.Priority)
            {
                Writer.WritePlainText(")");
            }
        }
        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            Writer.WritelnPlainText(";");
        }
        public override void VisitExpression(NameExpression expression)
        {
            Writer.WritePlainText(expression.Name);
        }
        public override void VisitExpression(Special expression)
        {
            Writer.WriteExpressionKeyword(expression.ToString());
        }
        public override void VisitExpression(MemberAccess expression)
        {
            VisitExpression(expression.Left);
            Writer.WritePlainText(".");
            VisitExpression(expression.Right);
        }

        public override void VisitExpression(BlockExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(AccordExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(MatchExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CastExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorCollection expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorTuple expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(IndexExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(InterpolatedString expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(InvokeGenericExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(FunctionDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorInstance expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(TernaryOperator expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorUsing expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(ReflectionExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitStatement(ForeachLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(DoWhileLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(FiniteLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(InfiniteLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(BaseLoopStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitExpression(ConditionExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitExpression(UsingExpression statement)
        {
            base.VisitExpression(statement);
        }

        public override void VisitStatement(BreakStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(ContinueStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitExpression(CreatorNewArray expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorContext expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorRange expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitStatement(TryStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitStatement(ThrowStatement statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitExpression(StaticExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(SliceExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(FunctionArgument expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NameConstraintDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NotConstraintDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(ConstraintExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(BaseInstanceCreator expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitStatement(CreatorContextBody statement)
        {
            Writer.WritelnPlainText(Writer.Unsupported(statement));
        }

        public override void VisitExpression(MakeGenericExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(MakeTemplateExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(ReferenceExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(DereferenceExpression expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary.DictionaryEntry expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }

        public override void VisitExpression(MacrosDefinition expression)
        {
            Writer.WritePlainText(Writer.Unsupported(expression));
        }
    }
}
