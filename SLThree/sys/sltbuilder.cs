using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public class sltbuilder
    {
        public abstract class AbstractBuilder
        {
            public abstract ExecutionContext.IExecutable BuildExecutable();
            public abstract void ApplyExpression(BaseExpression expression);
            public abstract void ApplyStatement(BaseStatement statement);
        }
        public abstract class AnyBuilder<T> : AbstractBuilder where T: ExecutionContext.IExecutable
        {
            public abstract T Build();
            public override ExecutionContext.IExecutable BuildExecutable() => Build();
        }
        public class CodeBuilder : AnyBuilder<BaseStatement>
        {
            private List<BaseStatement> statements = new List<BaseStatement>();

            public override BaseStatement Build()
            {
                if (statements.Count == 1) return statements[0];
                else return new StatementList(statements.ToArray(), statements[0].SourceContext);
            }

            public override void ApplyExpression(BaseExpression expression)
            {
                if (expression is BlockExpression blockExpression)
                    foreach (var x in blockExpression.Statements)
                        statements.Add(x);
                else statements.Add(new ExpressionStatement(expression, expression.SourceContext));
            }

            public override void ApplyStatement(BaseStatement statement)
            {
                statements.Add(statement);
            }
        }
        public class ExprBuilder : AnyBuilder<BaseExpression>
        {
            public abstract class AbstractOperator
            {
                public abstract BaseExpression apply(Stack<BaseExpression> stack);
                public static readonly SourceContext empty = new SourceContext();

                public static AbstractOperator Create(string op)
                {
                    switch (op)
                    {
                        case "?:": return new Ternary();
                        case "+": return new AnyOperator((x, y) => new BinaryAdd(x, y, empty), x => new UnaryAdd(x, empty));
                        case "-": return new AnyOperator((x, y) => new BinaryRem(x, y, empty), x => new UnaryRem(x, empty));
                        case "*": return new AnyOperator((x, y) => new BinaryMultiply(x, y, empty), x => new UnaryGetChooser(x, null, empty));
                        case "/": return new AnyOperator((x, y) => new BinaryDivide(x, y, empty));
                        case "^": return new AnyOperator((x, y) => new BinaryBitXor(x, y, empty), x => new UnaryChoose(x, empty));
                        case "==": return new AnyOperator((x, y) => new BinaryEquals(x, y, empty));
                    }
                    throw new ArgumentException();
                }
            }

            public class AnyOperator : AbstractOperator
            {
                public Func<BaseExpression, BaseExpression> Unary;
                public Func<BaseExpression, BaseExpression, BaseExpression> Binary;

                public AnyOperator(Func<BaseExpression, BaseExpression, BaseExpression> binary, Func<BaseExpression, BaseExpression> unary = null)
                {
                    Unary = unary;
                    Binary = binary;
                }

                public override BaseExpression apply(Stack<BaseExpression> stack)
                {
                    if (stack.Count == 1 && Unary != null)
                        return Unary(stack.Pop());
                    if (Binary != null)
                    {
                        var (val2, val1) = (stack.Pop(), stack.Pop());
                        return Binary(val1, val2);
                    }
                    throw new ArgumentException();
                }
            }
            public class Ternary : AbstractOperator
            {
                public override BaseExpression apply(Stack<BaseExpression> stack)
                {
                    var val1 = stack.Pop();
                    var val2 = stack.Pop();
                    var val3 = stack.Pop();
                    return new TernaryOperator(val1, val2, val3, new SourceContext(), false);
                }
            }

            internal Stack<object> values = new Stack<object>();

            public override BaseExpression Build()
            {
                var current_stack = new Stack<BaseExpression>();
                var values = new Queue<object>(this.values.Reverse());
                current_stack.Push((BaseExpression)values.Dequeue());
                while (values.Count > 0)
                {
                    var next = values.Dequeue();
                    if (next is AbstractOperator op)
                    {
                        var applied = op.apply(current_stack);
                        if (values.Count == 0) return applied;
                        else current_stack.Push(applied);
                    }
                    else if (values.Count == 0) return (BaseExpression)next;
                    else current_stack.Push((BaseExpression)next);
                }
                throw new ArgumentException();
            }

            public override void ApplyExpression(BaseExpression expression)
            {
                values.Push(expression);
            }

            public override void ApplyStatement(BaseStatement statement)
            {
                values.Push(new BlockExpression(statement is StatementList stl ? stl.Statements : new BaseStatement[1] { statement }, statement.SourceContext));
            }
        }

        public readonly Stack<AbstractBuilder> Executables = new Stack<AbstractBuilder>();
        public ExecutionContext.IExecutable result = null;

        public sltbuilder @operator(string str)
        {
            var ex = Executables.Peek();
            if (!(ex is ExprBuilder eb)) throw new ArgumentException("Inconvenient context for the operator");
            eb.values.Push(ExprBuilder.AbstractOperator.Create(str));
            return this;
        }

        public sltbuilder build()
        {
            var b = Executables.Pop();
            var executable = b.BuildExecutable();
            if (Executables.Count > 0)
            {
                var ex = Executables.Peek();
                switch (executable)
                {
                    case BaseStatement statement: ex.ApplyStatement(statement); break;
                    case BaseExpression expression: ex.ApplyExpression(expression); break;
                }
            }
            result = executable;
            return this;
        }

        public sltbuilder block()
        {
            Executables.Push(new CodeBuilder());
            return this;
        }

        public sltbuilder expr()
        {
            Executables.Push(new ExprBuilder());
            return this;
        }

        public sltbuilder apply(ExecutionContext.IExecutable executable)
        {
            var ex = Executables.Peek();
            switch (executable)
            {
                case BaseStatement statement: ex.ApplyStatement(statement); break;
                case BaseExpression expression: ex.ApplyExpression(expression); break;
            }
            return this;
        }
    }
#pragma warning restore IDE1006 // Стили именования
}
