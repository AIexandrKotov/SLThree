using SLThree.Extensions.Cloning;
using System;
using System.Collections.Generic;

namespace SLThree
{
    public class CreatorContextBody : StatementList, ICloneable
    {
        private (ExecutionContext, bool, int)[] Caches;

        public CreatorContextBody() : base() { }
        public CreatorContextBody(IList<BaseStatement> statements, SourceContext context) : base(statements, context)
        {
            foreach (var x in Statements)
                CheckOnContextStatements(x);
            Caches = new (ExecutionContext, bool, int)[Statements.Length];
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
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, assign.Right.GetValue(context), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
                        else
                            BinaryAssign.AssignToValue(target, assign.Left, assign.Right.GetValue(context), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
                    }
                    else if (es.Expression is CreatorContext creator)
                    {
                        if (creator.Name is MemberAccess @private)
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, creator.GetValue(target, context), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
                        else
                            BinaryAssign.AssignToValue(target, creator.Name, creator.GetValue(target, context), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
                    }
                    else if (es.Expression is FunctionDefinition function)
                    {
                        if (function.FunctionName is MemberAccess @private)
                            BinaryAssign.AssignToValue(target.@private.Context, @private.Right, function.GetValue(target.@private.Context), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
                        else 
                            BinaryAssign.AssignToValue(target, function.FunctionName, function.GetValue(target), ref Caches[i].Item1, ref Caches[i].Item2, ref Caches[i].Item3);
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
