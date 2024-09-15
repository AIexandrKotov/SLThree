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
            return restorator.sb.ToString();
        }
        public static string Restore<T>(BaseExpression expression, ExecutionContext context) where T : DefaultRestorator, new()
        {
            var restorator = GetRestorator<T>(context);
            restorator.VisitExpression(expression);
            return restorator.sb.ToString();
        }

        public virtual string LanguageName { get; } = "LANGNAME";
        public virtual int Tabulation { get; set; } = 4;

        public override void VisitStatement(StatementList statement)
        {
            Level += 1;
            Writeln(Unsupported(statement));
            Level -= 1;
        }

        public void WriteTab() => sb.Append(new string(' ', Tabulation * Level));
        public void Write(string s) => sb.Append(s);
        public void Writeln(string s) => sb.AppendLine(s);


        public override void VisitExpression(Literal expression)
        {
            Write(expression.ToString());
        }
        public override void VisitExpression(UnaryOperator expression)
        {
            Write($"{expression.Operator}");
            VisitExpression(expression.Left);
        }
        public override void VisitExpression(BinaryOperator expression)
        {
            VisitExpression(expression.Left);
            Write($" {expression.Operator} ");
            VisitExpression(expression.Right);
        }
        public override void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
            Writeln(";");
        }
        public override void VisitExpression(NameExpression expression)
        {
            Write(expression.Name);
        }
        public override void VisitExpression(Special expression)
        {
            Write(expression.ToString());
        }
        public override void VisitExpression(MemberAccess expression)
        {
            VisitExpression(expression.Left);
            Write(".");
            VisitExpression(expression.Right);
        }

        protected virtual string Unsupported(object o) => $"\"!{o.GetType().FullName}!\"";

        public override void VisitExpression(BlockExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(AccordExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(MatchExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CastExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorCollection expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorTuple expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(IndexExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(InterpolatedString expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(InvokeExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(InvokeGenericExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(FunctionDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorInstance expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(TernaryOperator expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorUsing expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(ReflectionExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(TypenameExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitStatement(ForeachLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(WhileLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(DoWhileLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(FiniteLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(InfiniteLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(BaseLoopStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitExpression(ConditionExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitStatement(ReturnStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitExpression(UsingExpression statement)
        {
            base.VisitExpression(statement);
        }

        public override void VisitStatement(BreakStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(ContinueStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitExpression(CreatorNewArray expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorContext expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorRange expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitStatement(TryStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitStatement(ThrowStatement statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitExpression(StaticExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(SliceExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(FunctionArgument expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NameConstraintDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitConstraint(TemplateMethod.NotConstraintDefinition expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(ConstraintExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(BaseInstanceCreator expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitStatement(CreatorContextBody statement)
        {
            Writeln(Unsupported(statement));
        }

        public override void VisitExpression(MakeGenericExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(MakeTemplateExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(ReferenceExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(DereferenceExpression expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(CreatorDictionary.DictionaryEntry expression)
        {
            Write(Unsupported(expression));
        }

        public override void VisitExpression(MacrosDefinition expression)
        {
            Write(Unsupported(expression));
        }

        protected StringBuilder sb = new StringBuilder();
        protected int Level = 0;
    }
}
