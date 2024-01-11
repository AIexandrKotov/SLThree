using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    public partial class Parser
    {
        public static readonly Parser This = new Parser();

        public BaseExpression ParseExpression(string s) => Parse("#EXPR# " + s).Cast<ExpressionStatement>().Expression;
        public object EvalExpression(string s, ExecutionContext context = null)
        {
            if (context == null) context = new ExecutionContext();
            return ParseExpression(s).GetValue(context);
        }

        public BaseStatement ParseScript(string s, string filename = null) => Parse("#SLT# " + s, filename);
        public ExecutionContext RunScript(string s, string filename = null, ExecutionContext context = null)
        {
            var parsed = ParseScript(s, filename);
            var ret = context ?? new ExecutionContext();
            parsed.GetValue(ret);
            return ret;
        }

        private static T Panic<T>(SLTException exception)
        {
            throw exception;
        }
        
        private static BaseStatement CheckOnContextStatements(BaseStatement statement)
        {
            if (statement is ExpressionStatement expressionStatement)
            {
                if (expressionStatement.Expression is ExpressionBinaryAssign)
                    return statement;
                throw new SyntaxError($"Expected assign expression, found {expressionStatement.Expression.GetType().Name}", expressionStatement.Expression.SourceContext);
            }
            if (statement is ContextStatement contextStatement)
                return contextStatement;
            throw new SyntaxError($"Expected assign expression, found {statement.GetType().Name}", statement.SourceContext);
        }
    }
}
