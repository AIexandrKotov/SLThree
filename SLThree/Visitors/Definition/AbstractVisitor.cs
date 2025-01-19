using SLThree.Extensions;
using System;
using System.Collections.Generic;

namespace SLThree.Visitors
{
    public abstract class AbstractVisitor : IVisitor
    {
        public AbstractVisitor()
        {
        }

        public virtual void VisitAny(object o)
        {
            switch (o)
            {
                case null: return;
                case BaseExpression expression: VisitExpression(expression); return;
                case BaseStatement statement: VisitStatement(statement); return;
                case TemplateMethod method: Visit(method); return;
                case GenericMethod method: Visit(method); return;
                case Method method: Visit(method); return;
                case ContextWrap context: Visit(context.Context); return;
                case ExecutionContext context: Visit(context); return;
            }
        }

        public virtual void Visit(Method method)
        {
            for (var i = 0; i < method.Statements.Statements.Length; i++)
                VisitStatement(method.Statements.Statements[i]);
        }
        public virtual void Visit(GenericMethod method)
        {
            for (var i = 0; i < method.Statements.Statements.Length; i++)
                VisitStatement(method.Statements.Statements[i]);
        }
        public virtual void Visit(TemplateMethod method)
        {
            for (var i = 0; i < method.Statements.Statements.Length; i++)
                VisitStatement(method.Statements.Statements[i]);
        }
        public virtual void Visit(ExecutionContext context)
        {
            foreach (var x in context.LocalVariables.Variables)
                VisitAny(x);
        }

        public List<ExecutionContext.IExecutable> Executables { get; } = new List<ExecutionContext.IExecutable>();

        private static Action<AbstractVisitor, BaseExpression> visitExpression = SLTHelpers.CreateInheritorSwitcher<AbstractVisitor, BaseExpression>("VisitExpression", 
                new Type[] { typeof(TemplateMethod.ConstraintDefinition), typeof(TypenameGenericPart), typeof(MemberAccess) },
                new Type[] { typeof(Literal), typeof(Special), typeof(BinaryOperator), typeof(UnaryOperator) }           
            );
        private static Action<AbstractVisitor, BaseStatement> visitStatement = SLTHelpers.CreateInheritorSwitcher<AbstractVisitor, BaseStatement>("VisitStatement",
                new Type[] { typeof(EmptyStatement) },
                new Type[] { }
            );
        private static Action<AbstractVisitor, TemplateMethod.ConstraintDefinition> visitConstraint = SLTHelpers.CreateInheritorSwitcher<AbstractVisitor, TemplateMethod.ConstraintDefinition>("VisitConstraint",
                new Type[] { typeof(TemplateMethod.ObjectConstraintDefinition) },
                new Type[] { }
            );
        public virtual void VisitStatement(BaseStatement statement)
        {
            Executables.Add(statement);
            visitStatement(this, statement);
            Executables.Remove(statement);
        }
        public virtual void VisitExpression(BaseExpression expression)
        {
            Executables.Add(expression);
            if (expression is TemplateMethod.ConstraintDefinition c) VisitConstraint(c);
            else if (expression is MemberAccess memberAccess) VisitExpression(memberAccess);
            else visitExpression(this, expression);
            Executables.Remove(expression);
        }
        public virtual void VisitConstraint(TemplateMethod.ConstraintDefinition expression)
        {
            Executables.Add(expression);
            visitConstraint(this, expression);
            Executables.Remove(expression);
        }

        public virtual void VisitExpression(AccordExpression expression)
        {
            for (var i = 0; i < expression.HeadAccords.Length; i++)
            {
                VisitExpression(expression.HeadAccords[i]);
            }
            for (var i = 0; i < expression.Accordings.Length; i++)
            {
                for (var j = 0; j > expression.Accordings[i].Length; j++)
                {
                    VisitExpression(expression.Accordings[i][j].Item1);
                    VisitConstraint(expression.Accordings[i][j].Item2);
                }
                VisitStatement(expression.Cases[i]);
            }
            if (expression.InDefault != null)
                VisitStatement(expression.InDefault);
        }
        public virtual void VisitExpression(MatchExpression expression)
        {
            VisitExpression(expression.Matching);
            for (var i = 0; i < expression.Matches.Length; i++)
            {
                for (var j = 0; j > expression.Matches[i].Length;j++)
                    VisitExpression(expression.Matches[i][j]);
                VisitStatement(expression.Cases[i]);
            }
            if (expression.InDefault != null)
                VisitStatement(expression.InDefault);
        }
        public virtual void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            Executables.Add(expression);
            VisitExpression(expression.Type);
            Executables.Remove(expression);
        }
        public virtual void VisitExpression(CreatorDictionary expression)
        {
            VisitExpression((BaseInstanceCreator)expression);
            foreach (var x in expression.Body)
            {
                VisitExpression(x.Key);
                VisitExpression(x.Value);
            }
        }
        public virtual void VisitExpression(CreatorCollection expression)
        {
            VisitExpression((BaseInstanceCreator)expression);
            foreach (var x in expression.Body)
                VisitExpression(x);
        }
        public virtual void VisitExpression(CreatorTuple expression)
        {
            foreach (var x in expression.Expressions)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(IndexExpression expression)
        {
            VisitExpression(expression.Expression);
            foreach (var x in expression.Arguments)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(InterpolatedString expression)
        {
            foreach (var x in expression.Expressions)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(InvokeExpression expression)
        {
            VisitExpression(expression.Left);
            foreach (var x in expression.Arguments)
            {
                VisitExpression(x);
            }
        }

        public virtual void VisitExpression(InvokeTemplateExpression expression)
        {
            VisitExpression(expression.Left);
            Executables.Add(expression);
            foreach (var x in expression.GenericArguments)
            {
                VisitExpression(x.Item1);
                VisitExpression(x.Item2);
            }
            Executables.Remove(expression);
            foreach (var x in expression.Arguments)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(InvokeGenericExpression expression)
        {
            VisitExpression(expression.Left);
            Executables.Add(expression);
            foreach (var x in expression.GenericArguments)
            {
                VisitExpression(x);
            }
            Executables.Remove(expression);
            foreach (var x in expression.Arguments)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(FunctionDefinition expression)
        {
            if (expression.FunctionName != null)
                VisitExpression(expression.FunctionName);
            foreach (var x in expression.GenericArguments)
            {
                VisitExpression(x.Item1);
                if (x.Item2 != null)
                    VisitConstraint(x.Item2);
            }
            foreach (var x in expression.Arguments)
            {
                VisitExpression(x.Name);
                if (x.DefaultValue != null)
                    VisitExpression(x.DefaultValue);
            }
            if (expression.ReturnTypeHint != null)
                VisitExpression(expression.ReturnTypeHint);
            VisitStatement(expression.FunctionBody);
        }

        public virtual void VisitExpression(NameExpression expression)
        {
            if (expression.TypeHint != null)
            {
                Executables.Add(expression);
                VisitExpression(expression.TypeHint);
                Executables.Remove(expression);
            }
        }

        public virtual void VisitExpression(CreatorInstance expression)
        {
            VisitExpression((BaseInstanceCreator)expression);
            if (expression.CreatorContext != null)
                VisitExpression(expression.CreatorContext);
        }

        public virtual void VisitExpression(Special expression)
        {

        }

        public virtual void VisitExpression(Literal expression)
        {

        }

        public virtual void VisitExpression(UnaryOperator expression)
        {
            if (expression.Left is TypenameExpression)
            {
                Executables.Add(expression);
                VisitExpression(expression.Left);
                Executables.Remove(expression);
            }
            else VisitExpression(expression.Left);
        }

        public virtual void VisitExpression(BinaryOperator expression)
        {
            VisitExpression(expression.Left);
            if (expression.Right is TypenameExpression)
            {
                Executables.Add(expression);
                VisitExpression(expression.Right);
                Executables.Remove(expression);
            }
            else VisitExpression(expression.Right);
        }

        public virtual void VisitExpression(TernaryOperator expression)
        {
            VisitExpression(expression.Condition);
            VisitExpression(expression.Left);
            VisitExpression(expression.Right);
        }

        public virtual void VisitExpression(MemberAccess expression)
        {
            VisitExpression(expression.Left);
            VisitExpression(expression.Right);
        }

        public virtual void VisitExpression(CreatorUsing expression)
        {
            VisitExpression(expression.Type);
        }

        public virtual void VisitExpression(ReflectionExpression expression)
        {
            VisitExpression(expression.Left);
            if (expression.Right != null)
                VisitExpression(expression.Right);
            if (expression.MethodArguments != null)
            {
                Executables.Add(expression);
                for (var i = 0; i < expression.MethodArguments.Length; i++)
                    VisitExpression(expression.MethodArguments[i]);
                Executables.Remove(expression);
            }
            if (expression.MethodGenericArguments != null)
            {
                Executables.Add(expression);
                for (var i = 0; i < expression.MethodGenericArguments.Length; i++)
                    VisitExpression(expression.MethodGenericArguments[i]);
                Executables.Remove(expression);
            }
        }

        public virtual void VisitExpression(TypenameExpression expression)
        {
            VisitExpression(expression.Typename);
            if (expression.Generics != null)
            {
                Executables.Add(expression);
                for (var i = 0; i < expression.Generics.Length; i++)
                    VisitExpression(expression.Generics[i]);
                Executables.Remove(expression);
            }
        }

        public BaseStatement PreviousStatement => throw new NotImplementedException();

        public virtual void VisitStatement(ForeachLoopStatement statement)
        {
            VisitExpression(statement.Left);
            VisitExpression(statement.Iterator);
            VisitStatement(statement as BaseLoopStatement);
        }

        public virtual void VisitStatement(WhileLoopStatement statement)
        {
            VisitExpression(statement.Condition);
            VisitStatement(statement as BaseLoopStatement);
        }

        public virtual void VisitStatement(DoWhileLoopStatement statement)
        {
            VisitExpression(statement.Condition);
            VisitStatement(statement as BaseLoopStatement);
        }

        public virtual void VisitStatement(FiniteLoopStatement statement)
        {
            VisitExpression(statement.Iterations);
            VisitStatement(statement as BaseLoopStatement);
        }

        public virtual void VisitStatement(InfiniteLoopStatement statement)
        {
            VisitStatement(statement as BaseLoopStatement);
        }

        public virtual void VisitStatement(BaseLoopStatement statement)
        {
            foreach (var x in statement.LoopBody)
                VisitStatement(x);
        }

        public virtual void VisitStatement(ExpressionStatement statement)
        {
            VisitExpression(statement.Expression);
        }

        public virtual void VisitExpression(ConditionExpression expression)
        {
            VisitExpression(expression.Condition);
            foreach (var x in expression.Body)
                VisitStatement(x);
        }

        public virtual void VisitStatement(ReturnStatement statement)
        {
            if (!statement.VoidReturn) VisitExpression(statement.Expression);
        }

        public virtual void VisitExpression(UsingExpression statement)
        {
            VisitExpression(statement.Using);
            if (statement.Alias != null)
                VisitExpression(statement.Alias);
        }

        public virtual void VisitStatement(StatementList statement)
        {
            foreach (var x in statement.Statements)
                VisitStatement(x);
        }

        public virtual void VisitStatement(BreakStatement statement)
        {

        }

        public virtual void VisitStatement(ContinueStatement statement)
        {

        }

        public virtual void VisitExpression(CreatorNewArray expression)
        {
            VisitExpression(expression.ArrayType);
            VisitExpression(expression.Size);
        }

        public virtual void VisitExpression(CreatorContext expression)
        {
            if (expression.HasName)
                VisitExpression(expression.Name);
            foreach (var x in expression.Ancestors)
                VisitExpression(x);
            if (expression.CreatorBody != null)
                VisitStatement(expression.CreatorBody);
        }

        public virtual void VisitExpression(CreatorRange expression)
        {
            if (expression.RangeType != null)
            {
                Executables.Add(expression);
                VisitExpression(expression.RangeType);
                Executables.Remove(expression);
            }
            VisitExpression(expression.LowerBound);
            VisitExpression(expression.UpperBound);
        }

        public virtual void VisitStatement(TryStatement statement)
        {
            for (var i = 0; i < statement.TryBody.Length; i++)
                VisitStatement(statement.TryBody[i]);
            if (statement.CatchVariable != null)
                VisitExpression(statement.CatchVariable);
            for (var i = 0; i < statement.CatchBody.Length; i++)
                VisitStatement(statement.CatchBody[i]);
            for (var i = 0; i < statement.FinallyBody.Length; i++)
                VisitStatement(statement.FinallyBody[i]);
        }

        public virtual void VisitStatement(ThrowStatement statement)
        {
            VisitExpression(statement.ThrowExpression);
        }

        public virtual void VisitExpression(StaticExpression expression)
        {
            VisitExpression(expression.Right);
        }

        public virtual void VisitExpression(SliceExpression expression)
        {
            VisitExpression(expression.Left);
            if (expression.LowerBound != null)
                VisitExpression(expression.LowerBound);
            if (expression.UpperBound != null)
                VisitExpression(expression.UpperBound);
        }

        public virtual void VisitExpression(BlockExpression expression)
        {
            for (var i = 0; i < expression.Statements.Length; i++)
                VisitStatement(expression.Statements[i]);
        }

        public virtual void VisitExpression(FunctionArgument expression)
        {
            VisitExpression(expression.Name);
            VisitExpression(expression.DefaultValue);
        }

        public virtual void VisitExpression(InvokeTemplateExpression.GenericMakingDefinition expression)
        {
            VisitExpression(expression.Expression);
        }

        public virtual void VisitConstraint(TemplateMethod.NameConstraintDefinition expression)
        {
            VisitExpression(expression.Name);
        }

        public virtual void VisitConstraint(TemplateMethod.FunctionConstraintDefinition expression)
        {
            VisitStatement(expression.Statement);
        }

        public virtual void VisitConstraint(TemplateMethod.CombineConstraintDefinition expression)
        {
            VisitConstraint(expression.Left);
            VisitConstraint(expression.Right);
        }

        public virtual void VisitConstraint(TemplateMethod.IntersectionConstraintDefinition expression)
        {
            VisitConstraint(expression.Left);
            VisitConstraint(expression.Right);
        }

        public virtual void VisitConstraint(TemplateMethod.NotConstraintDefinition expression)
        {
            VisitConstraint(expression.Left);
        }

        public virtual void VisitExpression(ConstraintExpression expression)
        {
            if (!expression.DefaultTarget)
                VisitExpression(expression.Target);
            if (expression.Name != null)
                VisitExpression(expression.Name);
            VisitConstraint(expression.Body);
        }

        public virtual void VisitExpression(BaseInstanceCreator expression)
        {
            Executables.Add(expression);
            VisitExpression(expression.Type);
            Executables.Remove(expression);
            if (expression.Name != null)
                VisitExpression(expression.Name);
            foreach (var x in expression.Arguments)
                VisitExpression(x);
        }

        public virtual void VisitStatement(CreatorContextBody statement)
        {
            VisitStatement(statement as StatementList);
        }

        public virtual void VisitExpression(MakeGenericExpression expression)
        {
            VisitExpression(expression.Left);
            Executables.Add(expression);
            foreach (var x in expression.GenericArguments)
            {
                VisitExpression(x);
            }
            Executables.Remove(expression);
        }

        public virtual void VisitExpression(MakeTemplateExpression expression)
        {
            VisitExpression(expression.Left);
            Executables.Add(expression);
            foreach (var x in expression.GenericArguments)
            {
                VisitExpression(x.Item1);
                VisitExpression(x.Item2);
            }
            Executables.Remove(expression);
        }

        public virtual void VisitExpression(ReferenceExpression expression)
        {
            VisitExpression(expression.Expression);
        }

        public virtual void VisitExpression(DereferenceExpression expression)
        {
            VisitExpression(expression.Expression);
        }

        public virtual void VisitExpression(CreatorDictionary.DictionaryEntry expression)
        {
            VisitExpression(expression.Key);
            VisitExpression(expression.Value);
        }

        public virtual void VisitExpression(MacrosDefinition expression)
        {
            VisitAny(expression.Executable);
        }
    }
}
