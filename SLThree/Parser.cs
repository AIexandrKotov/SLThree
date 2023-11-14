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
                if (expressionStatement.Lexem is ExpressionBinaryAssign)
                    return statement;
                throw new SyntaxError($"Expected assign expression, found {expressionStatement.Lexem.GetType().Name}", expressionStatement.Lexem.SourceContext);
            }
            throw new SyntaxError($"Expected assign expression, found {statement.GetType().Name}", statement.SourceContext);
        }
    }
}
