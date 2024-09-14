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
            base.VisitStatement(statement);
            Level -= 1;
        }

        public override void VisitConstraint(TemplateMethod.ConstraintDefinition constraint)
        {
            WriteTab();
            Writeln(Unsupported(constraint));
            WriteTab();
            Level += 1;
            base.VisitConstraint(constraint);
            Level -= 1;
        }

        public override void VisitStatement(BaseStatement statement)
        {
            WriteTab();
            Writeln(Unsupported(statement));
            WriteTab();
            Level += 1;
            base.VisitStatement(statement);
            Level -= 1;
        }

        public override void VisitExpression(BaseExpression expression)
        {
            WriteTab();
            Writeln(Unsupported(expression));
            WriteTab();
            Level += 1;
            base.VisitExpression(expression);
            Level -= 1;
        }

        public void WriteTab() => sb.Append(new string(' ', Tabulation * Level));
        public void Write(string s) => sb.Append(s);
        public void Writeln(string s) => sb.AppendLine(s);

        private string Unsupported(object o) => $"//!! {LanguageName} doesn't support {o.GetType().Name}";
        protected StringBuilder sb = new StringBuilder();
        protected int Level = 0;
    }
}
