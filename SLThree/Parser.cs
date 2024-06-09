using SLThree.Extensions;
using SLThree.Visitors;
using System.Collections.Generic;
using System.Linq;

namespace SLThree
{
    public partial class Parser
    {
        public static readonly Parser This = new Parser();

        public BaseExpression ParseExpression(string s) => this.Parse("#EXPR# " + s).Cast<ExpressionStatement>().Expression;
        public object EvalExpression(string s, ExecutionContext context = null)
        {
            if (context == null) context = new ExecutionContext();
            return ParseExpression(s).GetValue(context);
        }

        public BaseStatement ParseScript(string s, string filename = null) => this.Parse("#SLT# " + s, filename);
        public ExecutionContext RunScript(string s, string filename = null, ExecutionContext context = null)
        {
            var parsed = ParseScript(s, filename);
            var ret = context ?? new ExecutionContext();
            parsed.GetValue(ret);
            return ret;
        }

        private BaseStatement[] GetStatements(BaseStatement statement)
        {
            if (statement is StatementList list) return list.Statements;
            return new BaseStatement[1] { statement };
        }

        private StatementList GetListStatement(BaseStatement statement)
        {
            if (statement is StatementList list) return list;
            return new StatementList()
            {
                Statements = new BaseStatement[1] { statement },
                SourceContext = statement.SourceContext
            };
        }

        private T GetOptional<T>(IList<T> optional) where T : class
        {
            if (optional.Count == 1) return optional[0];
            return null;
        }

        private BaseExpression GetSpecialName(NameExpression expression)
        {
            switch (expression.Name)
            {
                case "base": return new BaseLiteral(expression.SourceContext);
                case "global": return new GlobalLiteral(expression.SourceContext);
                case "self": return new SelfLiteral(expression.SourceContext);
                case "this": return new ThisLiteral(expression.SourceContext);
                case "super": return new SuperLiteral(expression.SourceContext);
                case "upper": return new UpperLiteral(expression.SourceContext);
                case "private": return new PrivateLiteral(expression.SourceContext);
                case "true":
                case "false":
                    return new BoolLiteral(bool.Parse(expression.Name), expression.SourceContext);
                case "null":
                    return new NullLiteral(expression.SourceContext);
            }
            return expression;
        }

        private class InjectorVisitor : AbstractVisitor
        {
            public bool done;
            private readonly BaseExpression Addition;
            public InjectorVisitor(BaseExpression addition)
            {
                Addition = addition;
            }
            public override void VisitExpression(InvokeExpression expression)
            {
                if (done) return;
                var old_args = expression.Arguments;
                expression.Arguments = new BaseExpression[old_args.Length + 1];
                old_args.CopyTo(expression.Arguments, 1);
                expression.Arguments[0] = Addition;
                done = true;
            }
            public override void VisitExpression(InvokeGenericExpression expression)
            {
                if (done) return;
                var old_args = expression.Arguments;
                expression.Arguments = new BaseExpression[old_args.Length + 1];
                old_args.CopyTo(expression.Arguments, 1);
                expression.Arguments[0] = Addition;
                done = true;
            }
        }

        private static BaseExpression InjectFirst(BaseExpression left, BaseExpression right)
        {
            var iv = new InjectorVisitor(left);
            iv.VisitExpression(right);
            if (!iv.done) throw new SyntaxError("Right of |> operator must be invokation", right.SourceContext);
            return right;
        }

        private static T Panic<T>(SLTException exception)
        {
            throw exception;
        }
    }
}
