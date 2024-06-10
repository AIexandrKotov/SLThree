using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;

namespace SLThree
{
    public class CreatorContextBody : StatementList, ICloneable
    {
        private ExecutionContext counted_invoked;
        private bool is_name_expr;
        private int variable_index;

        public CreatorContextBody() : base() { }
        public CreatorContextBody(IList<BaseStatement> statements, SourceContext context) : base(statements, context)
        {
            foreach (var x in Statements)
                CheckOnContextStatements(x);
        }

        public ExecutionContext GetValue(ExecutionContext target, ExecutionContext context)
        {
            for (var i = 0; i < count; i++)
            {
                if (Statements[i] is ExpressionStatement es)
                {
                    if (es.Expression is BinaryAssign assign)
                    {
                        if (assign.Left is MemberAccess @private)
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, assign.Right.GetValue(context), ref counted_invoked, ref is_name_expr, ref variable_index);
                        else
                            BinaryAssign.AssignToValue(target, assign.Left, assign.Right.GetValue(context), ref counted_invoked, ref is_name_expr, ref variable_index);
                    }
                    else if (es.Expression is CreatorContext creator)
                    {
                        if (creator.Name is MemberAccess @private)
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, creator.GetValue(target, context), ref counted_invoked, ref is_name_expr, ref variable_index);
                        else
                            BinaryAssign.AssignToValue(target, creator.Name, creator.GetValue(target, context), ref counted_invoked, ref is_name_expr, ref variable_index);
                    }
                    else if (es.Expression is FunctionDefinition function)
                    {
                        if (function.FunctionName is MemberAccess @private)
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, function.GetValue(target.@private.Context), ref counted_invoked, ref is_name_expr, ref variable_index);
                        else 
                            BinaryAssign.AssignToValue(target, function.FunctionName, function.GetValue(target), ref counted_invoked, ref is_name_expr, ref variable_index);
                    }
                }
            }
            return target;
        }

        private static bool CheckName(BaseExpression expression)
        {
            if (expression is NameExpression) return true;
            if (expression is MemberAccess memberAccess && memberAccess.Right is NameExpression && memberAccess.Left is PrivateLiteral) return true;

            throw new LogicalError($"Expected name or private.name, found \"{expression}\"", expression.SourceContext);
        }

        private static BaseStatement CheckOnContextStatements(BaseStatement statement)
        {
            if (statement is ExpressionStatement expressionStatement)
            {
                if (expressionStatement.Expression is BinaryAssign assign)
                {
                    if (CheckName(assign.Left)) return statement;
                }
                if (expressionStatement.Expression is CreatorContext creator)
                {
                    if (creator.Name == null) throw new LogicalError($"Nested context definitions should be named", expressionStatement.Expression.SourceContext);
                    if (CheckName(creator.Name)) return statement;
                    return statement;
                }
                if (expressionStatement.Expression is FunctionDefinition function)
                {
                    if (function.FunctionName == null) throw new LogicalError($"Methods should be named", expressionStatement.Expression.SourceContext);
                    if (CheckName(function.FunctionName)) return statement;
                }
                throw new SyntaxError($"Expected assign/context/method, found \"{expressionStatement.Expression}\"", expressionStatement.Expression.SourceContext);
            }
            throw new SyntaxError($"Expected assign/context/method, found \"{statement}\"", statement.SourceContext);
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
