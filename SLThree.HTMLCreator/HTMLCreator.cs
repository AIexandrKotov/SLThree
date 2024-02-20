using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.HTMLCreator
{
    public class HTMLCreator : AbstractVisitor
    {
        public static readonly Dictionary<string, string> Replaces = new Dictionary<string, string>()
        {
            { "<", "&lt;" },
            { ">", "&gt;" }
        };
        public static string Replace(string s)
        {
            foreach (var x in Replaces) s = s.Replace(x.Key, x.Value);
            return s;
        }
        public static string GetSpan(string str, string classname)
        {
            return $"<span class=\"{classname}\">{str}</span>";
        }
        /// <summary>
        /// blue
        /// </summary>
        public static string GetKeyword1(string str) => GetSpan(str, "slt-keyword1"); //as is
        /// <summary>
        /// purple
        /// </summary>
        public static string GetKeyword2(string str) => GetSpan(str, "slt-keyword2"); //for while
        public static string GetTypeSpan(string str) => GetSpan(str, "slt-type");
        public static string GetDigit(string str) => GetSpan(str, "slt-digit");
        public static string GetString(string str) => GetSpan(str, "slt-string");
        public static string GetOperator(string str) => GetSpan(str, "slt-operator");
        public static string GetEscaped(string str)
        {
            return str.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\a", "\\a").Replace("\t", "\\t");
        }

        public string GetComment(string str)
        {
            return $"<span class=\"bqs-comment\">{Replace(str)}</span>";
        }
        public string Tab() => CurrentTab == 0 ? "" : Enumerable.Repeat("&nbsp;", CurrentTab * 4).JoinIntoString("");
        public void Newline()
        {
            CodeStrings.Add(CurrentString.ToString());
            CurrentString.Clear();
            CurrentString.Append(Tab());
        }

        public bool ListTupleElementNewLine = false;
        public bool ContextElementNewLine = true;
        public bool DictionaryElementNewLine = true;
        public StringBuilder CurrentString = new StringBuilder();
        public int CurrentTab = 0;
        public List<string> CodeStrings = new List<string>();


        public override void VisitExpression(Literal expression)
        {
            if (expression is StringLiteral)
                CurrentString.Append(GetString($"\"{GetEscaped(expression.Value.ToString())}\""));
            else if (expression is CharLiteral c)
                CurrentString.Append(GetString($"'{GetEscaped(c.ToString())}'"));
            else CurrentString.Append(GetDigit(expression.RawRepresentation));
        }
        public override void VisitExpression(BinaryOperator expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append($" {expression.Operator} ");
            VisitExpression(expression.Right);
        }
        public override void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append($" as ");
            VisitExpression(expression.Type);
        }
        public override void VisitExpression(TypenameExpression expression)
        {
            CurrentString.Append(GetTypeSpan(expression.Typename.ToString()));
            if (expression.Generics != null && expression.Generics.Length > 0)
            {
                CurrentString.Append(Replace("<"));
                foreach (var x in expression.Generics)
                    VisitExpression(x);
                CurrentString.Append(Replace(">"));
            }
        }
        public void VisitExpression(CreatorContext expression, bool newline, bool hasnew)
        {
            if (hasnew)
                CurrentString.Append(GetKeyword1("new context "));
            else 
                CurrentString.Append(GetKeyword1("context "));
            if (expression.HasName)
            {
                CurrentString.Append(expression.Name + " ");
            }
            if (expression.HasCast)
            {
                CurrentString.Append(": ");
                VisitExpression(expression.Typecast);
                CurrentString.Append(" ");
            }
            if (expression.HasBody)
            {
                CurrentString.Append("{ ");
                CurrentTab += 1;
                foreach (var x in expression.Body)
                {
                    if (newline) Newline();
                    else CurrentString.Append(" ");
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                if (newline) Newline();
                else CurrentString.Append(" ");
                CurrentString.Append("}");
            }
        }
        public override void VisitExpression(CreatorContext expression)
        {
            VisitExpression(expression, false, true);
        }
        public override void VisitExpression(MemberAccess expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append(".");
            VisitExpression(expression.Right);
        }
        public override void VisitExpression(InvokeExpression expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append("(");
            for (var i = 0; i < expression.Arguments.Length; i++)
            {
                VisitExpression(expression.Arguments[i]);
                if (i != expression.Arguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append(")");
        }
        public override void VisitExpression(InvokeGenericExpression expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append("<");
            for (var i = 0; i < expression.GenericArguments.Length; i++)
            {
                VisitExpression(expression.GenericArguments[i]);
                if (i != expression.GenericArguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append(">");
            CurrentString.Append("(");
            for (var i = 0; i < expression.Arguments.Length; i++)
            {
                VisitExpression(expression.Arguments[i]);
                if (i != expression.Arguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append(")");
        }
        public override void VisitExpression(CreatorList expression)
        {
            CurrentString.Append("[");
            if (!ListTupleElementNewLine)
            {
                for (var i = 0; i < expression.Expressions.Length; i++)
                {
                    var x = expression.Expressions[i];
                    VisitExpression(x);
                    if (i != expression.Expressions.Length - 1) CurrentString.Append(", ");
                }
            }
            else
            {
                CurrentTab += 1;
                foreach (var x in expression.Expressions)
                {
                    Newline();
                    VisitExpression(x);
                    CurrentString.Append(",");
                }
                CurrentTab -= 1;
                Newline();
            }
            CurrentString.Append("]");
        }
        public override void VisitExpression(CreatorArray expression)
        {
            CurrentString.Append("-[");
            if (!ListTupleElementNewLine)
            {
                for (var i = 0; i < expression.Expressions.Length; i++)
                {
                    var x = expression.Expressions[i];
                    VisitExpression(x);
                    if (i != expression.Expressions.Length - 1) CurrentString.Append(", ");
                }
            }
            else
            {
                foreach (var x in expression.Expressions)
                {
                    Newline();
                    VisitExpression(x);
                    CurrentString.Append(",");
                }
                Newline();
            }
            CurrentString.Append("]");
        }
        public override void VisitExpression(CreatorTuple expression)
        {
            CurrentString.Append("(");
            if (!ListTupleElementNewLine)
            {
                for (var i = 0; i < expression.Expressions.Length; i++)
                {
                    var x = expression.Expressions[i];
                    VisitExpression(x);
                    if (i != expression.Expressions.Length - 1) CurrentString.Append(", ");
                }
            }
            else
            {
                CurrentTab += 1;
                foreach (var x in expression.Expressions)
                {
                    Newline();
                    VisitExpression(x);
                    CurrentString.Append(",");
                }
                CurrentTab -= 1;
                Newline();
            }
            CurrentString.Append(")");
        }
        public override void VisitExpression(CreatorRange expression)
        {
            if (expression.RangeType != null)
            {
                CurrentString.Append(Replace("<"));
                VisitExpression(expression.RangeType);
                CurrentString.Append(Replace(">"));
            }
            VisitExpression(expression.LowerBound);
            CurrentString.Append("..");
            VisitExpression(expression.UpperBound);
        }
        public override void VisitExpression(CreatorDictionary expression)
        {
            CurrentString.Append("{");
            if (!DictionaryElementNewLine)
            {
                CurrentString.Append(" ");
                for (var i = 0; i < expression.Entries.Length; i++)
                {
                    var x = expression.Entries[i];
                    VisitExpression(x.Key);
                    CurrentString.Append(": ");
                    VisitExpression(x.Value);
                    if (i != expression.Entries.Length - 1) CurrentString.Append(", ");
                }
                CurrentString.Append(" ");
            }
            else
            {
                CurrentTab += 1;
                foreach (var x in expression.Entries)
                {
                    Newline();
                    VisitExpression(x.Key);
                    CurrentString.Append(": ");
                    VisitExpression(x.Value);
                    CurrentString.Append(",");
                }
                CurrentTab -= 1;
                Newline();
            }
            CurrentString.Append("}");
        }
        public override void VisitExpression(NameExpression expression)
        {
            if (expression.TypeHint != null)
            {
                VisitExpression(expression.TypeHint);
                CurrentString.Append(" ");
            }
            CurrentString.Append(expression.Name);
        }
        public override void VisitExpression(UnaryOperator expression)
        {
            CurrentString.Append($" {expression.Operator}");
            VisitExpression(expression.Left);
        }
        public override void VisitExpression(BaseExpression expression)
        {
            if (expression.PrioriryRaised) CurrentString.Append("(");
            base.VisitExpression(expression);
            if (expression.PrioriryRaised) CurrentString.Append(")");
        }
        public override void VisitExpression(IndexExpression expression)
        {
            VisitExpression(expression.Expression);
            CurrentString.Append("[");
            for (var i = 0; i < expression.Arguments.Length; i++)
            {
                VisitExpression(expression.Arguments[i]);
                if (i != expression.Arguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append("]");
        }
        public override void VisitExpression(CreatorNewArray expression)
        {
            CurrentString.Append(GetKeyword1("new "));
            VisitExpression(expression.ArrayType);
            CurrentString.Append("[");
            VisitExpression(expression.Size);
            CurrentString.Append("]");
        }
        public override void VisitExpression(TernaryOperator expression)
        {
            VisitExpression(expression.Condition);
            CurrentString.Append(" ? ");
            VisitExpression(expression.Left);
            CurrentString.Append(" : ");
            VisitExpression(expression.Right);
        }
        public override void VisitExpression(InterpolatedString expression)
        {
            var sb = CurrentString;
            CurrentString = new StringBuilder();

            var arr = new string[expression.Expressions.Length];
            for (var i = 0; i < expression.Expressions.Length; i++)
            {
                CurrentString.Clear();
                CurrentString.Append("<span class=\"slt-operator\">{");
                VisitExpression(expression.Expressions[i]);
                CurrentString.Append("}</span>");
                arr[i] = CurrentString.ToString();
            }
            CurrentString = sb;
            CurrentString.Append(GetString("$\"" + string.Format(expression.Value, arr) + "\""));
        }
        public override void VisitExpression(LambdaExpression expression)
        {
            foreach (var x in expression.Modificators)
            {
                CurrentString.Append(GetKeyword1(x));
                CurrentString.Append(" ");
            }
            CurrentString.Append("(");
            for (var i = 0; i < expression.Left.Arguments.Length; i++)
            {
                VisitExpression(expression.Left.Arguments[i]);
                if (i != expression.Left.Arguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append(")");
            if (expression.ReturnTypeHint != null)
            {
                CurrentString.Append(": ");
                VisitExpression(expression.ReturnTypeHint);
            }
            CurrentString.Append(" => ");
            if (expression.Right.Statements.Length == 1 && expression.Right.Statements[0] is ReturnStatement ret)
            {
                VisitExpression(ret.Expression);
            }
            else
            {
                CurrentString.Append(" {");
                CurrentTab += 1;
                foreach (var x in expression.Right.Statements)
                {
                    Newline();
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                Newline();
                CurrentString.Append("}");
            }
        }
        public void VisitExpression(CreatorUsing expression, bool hasnew)
        {
            if (hasnew)
                CurrentString.Append(GetKeyword1("new "));
            CurrentString.Append(GetKeyword1("using "));
            VisitExpression(expression.Type);
        }
        public override void VisitExpression(CreatorUsing expression)
        {
            VisitExpression(expression, true);
        }
        public override void VisitExpression(NewExpression expression)
        {
            CurrentString.Append(GetKeyword1("new "));
            VisitExpression(expression.Typename);
            CurrentString.Append("(");
            for (var i = 0; i < expression.Arguments.Length; i++)
            {
                VisitExpression(expression.Arguments[i]);
                if (i < expression.Arguments.Length - 1) CurrentString.Append(", ");
            }
            CurrentString.Append(")");
        }
        public override void VisitExpression(ReflectionExpression expression)
        {
            VisitExpression(expression.Left);
            CurrentString.Append("::");
            VisitExpression(expression.Right);
            if (expression.MethodGenericArguments?.Length > 0)
            {
                CurrentString.Append("<");
                for (var i = 0; i < expression.MethodGenericArguments.Length; i++)
                {
                    VisitExpression(expression.MethodGenericArguments[i]);
                    if (i < expression.MethodGenericArguments.Length - 1) CurrentString.Append(", ");
                }
                CurrentString.Append(">");
            }
            if (expression.MethodArguments?.Length > 0)
            {
                CurrentString.Append("(");
                for (var i = 0; i < expression.MethodArguments.Length; i++)
                {
                    VisitExpression(expression.MethodArguments[i]);
                    if (i < expression.MethodArguments.Length - 1) CurrentString.Append(", ");
                }

                CurrentString.Append(")");
            }
        }

        public override void VisitStatement(ContextStatement statement)
        {
            VisitExpression(statement.Creator, true, false);
        }
        public override void VisitStatement(BreakStatement statement)
        {
            CurrentString.Append(GetKeyword2("break") + ";");
        }
        public override void VisitStatement(ContinueStatement statement)
        {
            CurrentString.Append(GetKeyword2("continue") + ";");
        }
        public override void VisitStatement(UsingStatement statement)
        {
            VisitExpression(statement.Using, false);
            if (statement.Alias != null)
            {
                CurrentString.Append(GetKeyword1( " as "));
                CurrentString.Append(statement.Alias.Name);
            }
            CurrentString.Append(";");
        }
        public override void VisitStatement(TryStatement statement)
        {
            CurrentString.Append(GetKeyword2("try"));

            var trybody = statement.TryBody;
            CurrentString.Append(" {");
            CurrentTab += 1;
            foreach (var x in trybody)
            {
                Newline();
                VisitStatement(x);
            }
            CurrentTab -= 1;
            Newline();
            CurrentString.Append("}");

            var catchbody = statement.CatchBody;
            if (catchbody.Length == 0) return;
            CurrentString.Append(GetKeyword2(" catch"));
            if (statement.CatchVariable != null)
            {
                CurrentString.Append("(");
                VisitExpression(statement.CatchVariable);
                CurrentString.Append(")");
            }

            CurrentString.Append(" {");
            CurrentTab += 1;
            foreach (var x in catchbody)
            {
                Newline();
                VisitStatement(x);
            }
            CurrentTab -= 1;
            Newline();
            CurrentString.Append("}");

            var finallybody = statement.FinallyBody;
            if (finallybody.Length == 0) return;
            CurrentString.Append(GetKeyword2(" finally"));

            CurrentString.Append(" {");
            CurrentTab += 1;
            foreach (var x in finallybody)
            {
                Newline();
                VisitStatement(x);
            }
            CurrentTab -= 1;
            Newline();
            CurrentString.Append("}");
        }
        public override void VisitStatement(ThrowStatement statement)
        {
            CurrentString.Append(GetKeyword2("throw"));
            if (statement.ThrowExpression != null)
            {
                CurrentString.Append(" ");
                VisitExpression(statement.ThrowExpression);
            }
            CurrentString.Append(";");
        }
        public override void VisitStatement(ReturnStatement statement)
        {
            CurrentString.Append(GetKeyword2("return"));
            if (statement.Expression != null)
            {
                CurrentString.Append(" ");
                VisitExpression(statement.Expression);
            }
            CurrentString.Append(";");
        }
        public override void VisitStatement(ForeachLoopStatement statement)
        {
            CurrentString.Append(GetKeyword2("foreach") + " (");
            VisitExpression(statement.Left);
            CurrentString.Append(GetKeyword2(" in "));
            VisitExpression(statement.Iterator);
            CurrentString.Append(")");

            if (statement.LoopBody.Length < 2)
            {
                CurrentTab += 1;
                Newline();
                CurrentTab -= 1;
                VisitStatement(statement.LoopBody[0]);
            }
            else
            {
                CurrentString.Append(" {");
                CurrentTab += 1;
                foreach (var x in statement.LoopBody)
                {
                    Newline();
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                Newline();
                CurrentString.Append("}");
            }
        }
        public override void VisitStatement(ConditionStatement statement)
        {
            CurrentString.Append(GetKeyword2("if") + " (");
            VisitExpression(statement.Condition);
            CurrentString.Append(")");

            var ifbody = statement.IfBody;
            if (ifbody.Length < 2)
            {
                CurrentTab += 1;
                Newline();
                CurrentTab -= 1;
                VisitStatement(ifbody[0]);
            }
            else
            {
                CurrentString.Append(" {");
                CurrentTab += 1;
                foreach (var x in ifbody)
                {
                    Newline();
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                Newline();
                CurrentString.Append("}");
            }


            var elsebody = statement.ElseBody;
            if (elsebody.Length == 0) return;
            Newline();
            CurrentString.Append(GetKeyword2("else "));

            if (elsebody.Length < 2)
            {
                CurrentTab += 1;
                Newline();
                CurrentTab -= 1;
                VisitStatement(elsebody[0]);
            }
            else
            {
                CurrentString.Append(" {");
                CurrentTab += 1;
                foreach (var x in elsebody)
                {
                    Newline();
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                Newline();
                CurrentString.Append("}");
            }
        }
        public override void VisitStatement(WhileLoopStatement statement)
        {
            CurrentString.Append(GetKeyword2("while") + " (");
            VisitExpression(statement.Condition);
            CurrentString.Append(")");

            if (statement.LoopBody.Length < 2)
            {
                CurrentTab += 1;
                Newline();
                CurrentTab -= 1;
                VisitStatement(statement.LoopBody[0]);
            }
            else
            {
                CurrentString.Append(" {");
                CurrentTab += 1;
                foreach (var x in statement.LoopBody)
                {
                    Newline();
                    VisitStatement(x);
                }
                CurrentTab -= 1;
                Newline();
                CurrentString.Append("}");
            }
        }
        public override void VisitStatement(ExpressionStatement statement)
        {
            base.VisitExpression(statement.Expression);
            CurrentString.Append(";");
        }

        public string GetHTMLCode(BaseStatement[] statements)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class=\"slt-code\"><ol>");

            foreach (var line in statements)
            {
                Newline();
                VisitStatement(line);
            }
            
            if (CurrentString.Length > 0) CodeStrings.Add(CurrentString.ToString());
            CurrentString.Clear();

            foreach (var line in CodeStrings)
            {
                sb.Append("<li class=\"slt-line\">");
                sb.Append(line);
                sb.AppendLine("</li>");
            }

            sb.AppendLine("</ol>\n</div>");
            return sb.ToString();
        }
    }
}
