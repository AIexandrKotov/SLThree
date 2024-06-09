using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;

namespace SLThree
{
    public class CreatorContextBody : StatementList, ICloneable
    {
        public CreatorContextBody() : base() { }
        public CreatorContextBody(IList<BaseStatement> statements, SourceContext context) : base(statements, context)
        {
            foreach (var x in Statements)
                CheckOnContextStatements(x);
        }

        public ExecutionContext GetValue(ExecutionContext target, ExecutionContext call)
        {
            for (var i = 0; i < count; i++)
            {
                if (Statements[i] is ExpressionStatement es && es.Expression is BinaryAssign assign)
                    assign.AssignValue(target, assign.Left, assign.Right.GetValue(call));
            }
            return target;
        }

        private static BaseStatement CheckOnContextStatements(BaseStatement statement)
        {
            if (statement is ExpressionStatement expressionStatement)
            {
                if (expressionStatement.Expression is BinaryAssign)
                    return statement;
                throw new SyntaxError($"Expected assign expression, found {expressionStatement.Expression.GetType().Name}", expressionStatement.Expression.SourceContext);
            }
            throw new SyntaxError($"Expected assign expression, found {statement.GetType().Name}", statement.SourceContext);
        }

        public override object GetValue(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new CreatorContextBody(Statements.CloneArray(), SourceContext.CloneCast());
        }
    }
}
