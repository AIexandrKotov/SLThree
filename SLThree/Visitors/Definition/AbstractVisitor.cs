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
                case ExecutionContext context: Visit(context); return;
                case GenericMethod method: Visit(method); return;
                case Method method: Visit(method); return;
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
        public virtual void Visit(ExecutionContext context)
        {
            foreach (var x in context.LocalVariables.Variables)
                VisitAny(x);
        }

        public List<ExecutionContext.IExecutable> Executables { get; } = new List<ExecutionContext.IExecutable>();

        public virtual void VisitExpression(BaseExpression expression)
        {
            Executables.Add(expression);
            switch (expression)
            {
                case CastExpression expr: VisitExpression(expr); return;
                case MemberAccess expr: VisitExpression(expr); return;
                case TernaryOperator expr: VisitExpression(expr); return;
                case BinaryOperator expr: VisitExpression(expr); return;
                case UnaryOperator expr: VisitExpression(expr); return;
                case Special expr: VisitExpression(expr); return;
                case Literal expr: VisitExpression(expr); return;
                case CreatorInstance expr: VisitExpression(expr); return;
                case NameExpression expr: VisitExpression(expr); return;
                case ConditionExpression expr: VisitExpression(expr); return;
                case FunctionDefinition expr: VisitExpression(expr); return;
                case StaticExpression expr: VisitExpression(expr); return;
                case InvokeExpression expr: VisitExpression(expr); return;
                case InvokeGenericExpression expr: VisitExpression(expr); return;
                case InterpolatedString expr: VisitExpression(expr); return;
                case IndexExpression expr: VisitExpression(expr); return;
                case CreatorTuple expr: VisitExpression(expr); return;
                case CreatorDictionary expr: VisitExpression(expr); return;
                case CreatorList expr: VisitExpression(expr); return;
                case CreatorUsing expr: VisitExpression(expr); return;
                case ReflectionExpression expr: VisitExpression(expr); return;
                case TypenameExpression expr: VisitExpression(expr); return;
                case CreatorNewArray expr: VisitExpression(expr); return;
                case CreatorArray expr: VisitExpression(expr); return;
                case CreatorContext expr: VisitExpression(expr); return;
                case CreatorRange expr: VisitExpression(expr); return;
                case MatchExpression expr: VisitExpression(expr); return;
            }
            Executables.Remove(expression);
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
        public virtual void VisitExpression(CreatorList expression)
        {
            if (expression.ListType != null)
                VisitExpression(expression.ListType);
            foreach (var x in expression.Expressions)
            {
                VisitExpression(x);
            }
        }

        public virtual void VisitExpression(CreatorArray expression)
        {
            if (expression.ListType != null)
                VisitExpression(expression.ListType);
            foreach (var x in expression.Expressions)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(CreatorDictionary expression)
        {
            if (expression.DictionaryType != null)
            {
                VisitExpression(expression.DictionaryType[0]);
                VisitExpression(expression.DictionaryType[1]);
            }
            foreach (var x in expression.Entries)
            {
                VisitExpression(x.Key);
                VisitExpression(x.Value);
            }
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
            Visit(expression.Method);
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
            Executables.Add(expression);
            VisitExpression(expression.Type);
            Executables.Remove(expression);
            if (expression.Name != null)
                VisitExpression(expression.Name);
            foreach (var x in expression.Arguments)
                VisitExpression(x);
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
            if (expression.Generics != null)
            {
                Executables.Add(expression);
                for (var i = 0; i < expression.Generics.Length; i++)
                    VisitExpression(expression.Generics[i]);
                Executables.Remove(expression);
            }
        }

        public BaseStatement PreviousStatement => throw new NotImplementedException();

        public virtual void VisitStatement(BaseStatement statement)
        {
            Executables.Add(statement);
            switch (statement)
            {
                case ForeachLoopStatement st: VisitStatement(st); return;
                case WhileLoopStatement st: VisitStatement(st); return;
                case ExpressionStatement st: VisitStatement(st); return;
                case ReturnStatement st: VisitStatement(st); return;
                case UsingStatement st: VisitStatement(st); return;
                case StatementList st: VisitStatement(st); return;
                case BreakStatement st: VisitStatement(st); return;
                case ContinueStatement st: VisitStatement(st); return;
                case TryStatement st: VisitStatement(st); return;
                case ThrowStatement st: VisitStatement(st); return;
            }
            Executables.Remove(statement);
        }

        public virtual void VisitStatement(ForeachLoopStatement statement)
        {
            VisitExpression(statement.Left);
            VisitExpression(statement.Iterator);
            foreach (var x in statement.LoopBody)
                VisitStatement(x);
        }

        public virtual void VisitStatement(WhileLoopStatement statement)
        {
            VisitExpression(statement.Condition);
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

        public virtual void VisitStatement(UsingStatement statement)
        {
            VisitExpression(statement.Using);
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

        public void VisitExpression(StaticExpression expression)
        {
            VisitExpression(expression.Right);
        }
    }
}
