using SLThree.Visitors;
using System.Linq.Expressions;
using System.Text;

namespace SLThree.Metadata
{
    public class DefaultRestorator : AbstractVisitor
    {
        public static T GetRestorator<T>(ExecutionContext context = null) where T: DefaultRestorator, new()
        {
            return context?.Unwrap<T>() ?? new T();
        }

        public static string Restore<T>(BaseStatement statement, ExecutionContext context) where T: DefaultRestorator, new()
        {
            var restorator = GetRestorator<T>(context);
            restorator.VisitStatement(statement);
            restorator.RemoveEndNewlines();
            return restorator.sb.ToString();
        }
        public static string Restore<T>(BaseExpression expression, ExecutionContext context) where T : DefaultRestorator, new()
        {
            var restorator = GetRestorator<T>(context);
            restorator.VisitExpression(expression);
            restorator.RemoveEndNewlines();
            return restorator.sb.ToString();
        }

        private void RemoveEndNewlines()
        {
            while (sb[sb.Length - 1] == '\n' || sb[sb.Length - 1] == '\r')
                sb.Remove(sb.Length - 1, 1);
        }

        public virtual string LanguageName { get; } = "LANGNAME";
        public virtual int Tabulation { get; set; } = 4;

        public override void VisitStatement(StatementList statement)
        {
            Level += 1;
            WritelnPlainText(Unsupported(statement));
            Level -= 1;
        }

        public virtual void WriteTab() => sb.Append(new string(' ', Tabulation * Level));
        public virtual void WritePlainText(string s) => sb.Append(s);
        public virtual void WriteTypeText(string s) => sb.Append(s);
        public virtual void WriteCallText(string s) => sb.Append(s);
        public virtual void WriteExpressionKeyword(string s) => sb.Append(s);
        public virtual void WriteStatementKeyword(string s) => sb.Append(s);
        public virtual void WritelnPlainText(string s) => sb.AppendLine(s);
        public virtual void Writeln() => sb.AppendLine();

        public override void VisitExpression(BaseExpression expression)
        {
            if (expression.PrioriryRaised) WritePlainText("(");
            base.VisitExpression(expression);
            if (expression.PrioriryRaised) WritePlainText(")");
        }

        public override void VisitExpression(Literal expression)
        {
            WritePlainText(expression.RawRepresentation);
        }
        public override void VisitExpression(UnaryOperator expression)
        {
            WritePlainText($"{expression.Operator}");
            VisitExpression(expression.Left);
        }
        public override void VisitExpression(BinaryOperator expression)
        {
            VisitExpression(expression.Left);
            WritePlainText($" {expression.Operator} ");
            VisitExpression(expression.Right);
        }
        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            WritelnPlainText(";");
        }
        public override void VisitExpression(NameExpression expression)
        {
            WritePlainText(expression.Name);
        }
        public override void VisitExpression(Special expression)
        {
            WriteExpressionKeyword(expression.ToString());
        }
        public override void VisitExpression(MemberAccess expression)
        {
            VisitExpression(expression.Left);
            WritePlainText(".");
            VisitExpression(expression.Right);
        }

        protected virtual string Unsupported(object o) => $"\"!{o.GetType().FullName}!\"";

        public override void VisitExpression(BlockExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(AccordExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(MatchExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CastExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorCollection expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorTuple expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(IndexExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(InterpolatedString expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(InvokeGenericExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(FunctionDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorInstance expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(TernaryOperator expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorUsing expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(ReflectionExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitStatement(ForeachLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(DoWhileLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(FiniteLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(InfiniteLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(BaseLoopStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitExpression(ConditionExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitExpression(UsingExpression statement)
        {
            base.VisitExpression(statement);
        }

        public override void VisitStatement(BreakStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(ContinueStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitExpression(CreatorNewArray expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorContext expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorRange expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitStatement(TryStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitStatement(ThrowStatement statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitExpression(StaticExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(SliceExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(FunctionArgument expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NameConstraintDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NotConstraintDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(ConstraintExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(BaseInstanceCreator expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitStatement(CreatorContextBody statement)
        {
            WritelnPlainText(Unsupported(statement));
        }

        public override void VisitExpression(MakeGenericExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(MakeTemplateExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(ReferenceExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(DereferenceExpression expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary.DictionaryEntry expression)
        {
            WritePlainText(Unsupported(expression));
        }

        public override void VisitExpression(MacrosDefinition expression)
        {
            WritePlainText(Unsupported(expression));
        }

        protected StringBuilder sb = new StringBuilder();
        protected int Level = 0;
    }
}
