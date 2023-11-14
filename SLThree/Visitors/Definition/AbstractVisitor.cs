using System;
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
                case BaseLexem lexem: VisitLexem(lexem); return;
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

        public virtual void VisitLexem(BaseLexem lexem)
        {
            Executables.Add(lexem);
            switch (lexem)
            {
                case CastLexem lex: VisitLexem(lex); return;
                case ChanceChooseLexem lex: VisitLexem(lex); return;
                case MemberAccess lex: VisitLexem(lex); return;
                case ExpressionTernary lex: VisitLexem(lex); return;
                case ExpressionBinary lex: VisitLexem(lex); return;
                case ExpressionUnary lex: VisitLexem(lex); return;
                case Literal lex: VisitLexem(lex); return;
                case TypeofLexem lex: VisitLexem(lex); return;
                case NewLexem lex: VisitLexem(lex); return;
                case NameLexem lex: VisitLexem(lex); return;
                case LambdaLexem lex: VisitLexem(lex); return;
                case InvokeLexem lex: VisitLexem(lex); return;
                case InterpolatedString lex: VisitLexem(lex); return;
                case IndexLexem lex: VisitLexem(lex); return;
                case EqualchanceChooseLexem lex: VisitLexem(lex); return;
                case CreatorTuple lex: VisitLexem(lex); return;
                case CreatorDictionary lex: VisitLexem(lex); return;
                case CreatorArray lex: VisitLexem(lex); return;
            }
            Executables.Remove(lexem);
        }

        public virtual void VisitLexem(CastLexem lexem)
        {
            VisitLexem(lexem.Left);
            VisitLexem(lexem.Right);
        }
        public virtual void VisitLexem(ChanceChooseLexem lexem)
        {
            foreach (var x in lexem.Chooser)
            {
                VisitLexem(x.Item1);
                VisitLexem(x.Item2);
            }
        }
        public virtual void VisitLexem(CreatorArray lexem)
        {
            foreach (var x in lexem.Lexems)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(CreatorDictionary lexem)
        {
            foreach (var x in lexem.Entries)
            {
                VisitLexem(x.Key);
                VisitLexem(x.Value);
            }
        }
        public virtual void VisitLexem(CreatorTuple lexem)
        {
            foreach (var x in lexem.Lexems)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(EqualchanceChooseLexem lexem)
        {
            foreach (var x in lexem.Chooser)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(IndexLexem lexem)
        {
            VisitLexem(lexem.Lexem);
            foreach (var x in lexem.Arguments)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(InterpolatedString lexem)
        {
            foreach (var x in lexem.Lexems)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(InvokeLexem lexem)
        {
            VisitLexem(lexem.Left);
            foreach (var x in lexem.Arguments)
            {
                VisitLexem(x);
            }
        }
        public virtual void VisitLexem(LambdaLexem lexem)
        {
            Visit(lexem.Method);
        }

        public virtual void VisitLexem(NameLexem lexem)
        {
            
        }

        public virtual void VisitLexem(NewLexem lexem)
        {
            if (lexem.MemberAccess != null) VisitLexem(lexem.MemberAccess);
            else VisitLexem(lexem.InvokeLexem);
        }

        public virtual void VisitLexem(TypeofLexem lexem)
        {
            
        }

        public virtual void VisitLexem(Literal lexem)
        {
            
        }

        public virtual void VisitLexem(ExpressionUnary lexem)
        {
            VisitLexem(lexem.Left);
        }

        public virtual void VisitLexem(ExpressionBinary lexem)
        {
            VisitLexem(lexem.Left);
            VisitLexem(lexem.Right);
        }

        public virtual void VisitLexem(ExpressionTernary lexem)
        {
            VisitLexem(lexem.Condition);
            VisitLexem(lexem.Left);
            VisitLexem(lexem.Right);
        }

        public virtual void VisitLexem(MemberAccess lexem)
        {
            VisitLexem(lexem.Left);
            VisitLexem(lexem.Right);
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
            }
            Executables.Remove(statement);
        }

        public virtual void VisitStatement(ForeachLoopStatement statement)
        {
            VisitLexem(statement.Iterator);
            foreach (var x in statement.LoopBody)
                VisitStatement(x);
        }

        public virtual void VisitStatement(WhileLoopStatement statement)
        {
            VisitLexem(statement.Condition);
            foreach (var x in statement.LoopBody)
                VisitStatement(x);
        }

        public virtual void VisitStatement(ExpressionStatement statement)
        {
            VisitLexem(statement.Lexem);
        }

        public virtual void VisitStatement(ConditionStatement statement)
        {
            VisitLexem(statement.Condition);
            foreach (var x in statement.Body)
                VisitStatement(x);
        }

        public virtual void VisitStatement(ReturnStatement statement)
        {
            if (!statement.VoidReturn) VisitLexem(statement.Lexem);
        }

        public virtual void VisitStatement(SwitchStatement statement)
        {
            VisitLexem(statement.Value);
            foreach (var x in statement.Cases)
            {
                VisitLexem(x.Value);
                VisitStatement(x.Statements);
            }
        }

        public virtual void VisitStatement(UsingStatement statement)
        {
            
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
    }
}
