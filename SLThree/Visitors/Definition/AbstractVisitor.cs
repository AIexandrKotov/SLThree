﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                case Method method: Visit(method); return;
            }
        }

        public virtual void Visit(Method method)
        {
            if (method is RecursiveMethod rm) Visit(rm);
            else
            {
                for (var i = 0; i < method.Statements.Statements.Length; i++)
                    VisitStatement(method.Statements.Statements[i]);
            }
        }
        public virtual void Visit(RecursiveMethod method)
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
                case Literal expr: VisitExpression(expr); return;
                case NewExpression expr: VisitExpression(expr); return;
                case NameExpression expr: VisitExpression(expr); return;
                case LambdaExpression expr: VisitExpression(expr); return;
                case InvokeExpression expr: VisitExpression(expr); return;
                case InterpolatedString expr: VisitExpression(expr); return;
                case IndexExpression expr: VisitExpression(expr); return;
                case CreatorTuple expr: VisitExpression(expr); return;
                case CreatorDictionary expr: VisitExpression(expr); return;
                case CreatorList expr: VisitExpression(expr); return;
                case CreatorUsing expr: VisitExpression(expr); return;
                case ReflectionExpression expr: VisitExpression(expr); return;
                case TypenameExpression expr: VisitExpression(expr); return;
                case CreatorArray expr: VisitExpression(expr); return;
                case CreatorContext expr: VisitExpression(expr); return;
                case CreatorRange expr: VisitExpression(expr); return;
            }
            Executables.Remove(expression);
        }

        public virtual void VisitExpression(CastExpression expression)
        {
            VisitExpression(expression.Left);
            VisitExpression(expression.Type);
        }
        public virtual void VisitExpression(CreatorList expression)
        {
            foreach (var x in expression.Expressions)
            {
                VisitExpression(x);
            }
        }
        public virtual void VisitExpression(CreatorDictionary expression)
        {
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
        public virtual void VisitExpression(LambdaExpression expression)
        {
            Visit(expression.Method);
        }

        public virtual void VisitExpression(NameExpression expression)
        {
            
        }

        public virtual void VisitExpression(NewExpression expression)
        {
            VisitExpression(expression.Typename);
            for (var i = 0; i < expression.Arguments.Length; i++)
                VisitExpression(expression.Arguments[i]);
        }

        public virtual void VisitExpression(Literal expression)
        {
            
        }

        public virtual void VisitExpression(UnaryOperator expression)
        {
            VisitExpression(expression.Left);
        }

        public virtual void VisitExpression(BinaryOperator expression)
        {
            VisitExpression(expression.Left);
            VisitExpression(expression.Right);
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
                for (var i = 0; i < expression.MethodArguments.Length; i++)
                    VisitExpression(expression.MethodArguments[i]);
            if (expression.MethodGenericArguments != null)
                for (var i = 0; i < expression.MethodGenericArguments.Length; i++)
                    VisitExpression(expression.MethodGenericArguments[i]);
        }

        public virtual void VisitExpression(TypenameExpression expression)
        {
            if (expression.Generics != null)
                for (var i = 0; i < expression.Generics.Length; i++)
                    VisitExpression(expression.Generics[i]);
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
                case ConditionStatement st: VisitStatement(st); return;
                case ReturnStatement st: VisitStatement(st); return;
                case SwitchStatement st: VisitStatement(st); return;
                case UsingStatement st: VisitStatement(st); return;
                case StatementListStatement st: VisitStatement(st); return;
                case BreakStatement st: VisitStatement(st); return;
                case ContinueStatement st: VisitStatement(st); return;
                case TryStatement st: VisitStatement(st); return;
                case ThrowStatement st: VisitStatement(st); return;
            }
            Executables.Remove(statement);
        }

        public virtual void VisitStatement(ForeachLoopStatement statement)
        {
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

        public virtual void VisitStatement(ConditionStatement statement)
        {
            VisitExpression(statement.Condition);
            foreach (var x in statement.Body)
                VisitStatement(x);
        }

        public virtual void VisitStatement(ReturnStatement statement)
        {
            if (!statement.VoidReturn) VisitExpression(statement.Expression);
        }

        public virtual void VisitStatement(SwitchStatement statement)
        {
            VisitExpression(statement.Value);
            foreach (var x in statement.Cases)
            {
                VisitExpression(x.Value);
                VisitStatement(x.Statements);
            }
        }

        public virtual void VisitStatement(UsingStatement statement)
        {
            VisitExpression(statement.Using);
        }

        public virtual void VisitStatement(StatementListStatement statement)
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

        public virtual void VisitExpression(CreatorArray expression)
        {
            VisitExpression(expression.ArrayType);
            VisitExpression(expression.Size);
        }

        public virtual void VisitExpression(CreatorContext expression)
        {
            if (expression.Typecast != null) VisitExpression(expression.Typecast);
            if (expression.Body != null)
                foreach (var x in expression.Body)
                    VisitStatement(x);
        }

        public virtual void VisitExpression(CreatorRange expression)
        {
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
    }
}
