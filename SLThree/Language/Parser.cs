using Pegasus.Common;
using SLThree.Extensions;
using SLThree.Extensions.Cloning;
using SLThree.Metadata;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.Language
{
    public partial class Parser : IParser
    {
        public static readonly Parser This = new Parser();
        public SourceContext _(Cursor cursor) => new SourceContext(cursor);
        private static T Panic<T>(SLTException exception)
        {
            throw exception;
        }

        public BaseExpression ParseExpression(string s, string filename = null)
        {
            var ret = this.Parse("#EXPR# " + s).Cast<ExpressionStatement>().Expression;
            return ret;
        }
        public BaseStatement ParseScript(string s, string filename = null)
        {
            var ret = this.Parse("#SLT# " + s, filename);
            return ret;
        }


        private BaseStatement[] GetStatements(BaseStatement statement)
        {
            if (statement is StatementList list) return list.Statements;
            return new BaseStatement[1] { statement };
        }

        private StatementList GetListStatement(BaseStatement statement)
        {
            if (statement is StatementList list) return list;
            return new StatementList()
            {
                Statements = new BaseStatement[1] { statement },
                SourceContext = statement.SourceContext
            };
        }

        private T GetOptional<T>(IList<T> optional) where T : class
        {
            if (optional.Count == 1) return optional[0];
            return null;
        }

        private BaseExpression GetSpecialName(NameExpression expression)
        {
            switch (expression.Name)
            {
                case "parent": return new ParentLiteral(expression.SourceContext);
                case "global": return new GlobalLiteral(expression.SourceContext);
                case "self": return new SelfLiteral(expression.SourceContext);
                case "this": return new ThisLiteral(expression.SourceContext);
                case "super": return new SuperLiteral(expression.SourceContext);
                case "upper": return new UpperLiteral(expression.SourceContext);
                case "private": return new PrivateLiteral(expression.SourceContext);
                case "true":
                case "false":
                    return new BoolLiteral(bool.Parse(expression.Name), expression.Name, expression.SourceContext);
                case "null":
                    return new NullLiteral(expression.SourceContext);
            }
            return expression;
        }

        private class InjectorVisitor : AbstractVisitor
        {
            public bool done;
            private readonly BaseExpression Addition;
            public InjectorVisitor(BaseExpression addition)
            {
                Addition = addition;
            }
            public override void VisitExpression(InvokeExpression expression)
            {
                if (done) return;
                var old_args = expression.Arguments;
                expression.Arguments = new BaseExpression[old_args.Length + 1];
                old_args.CopyTo(expression.Arguments, 1);
                expression.Arguments[0] = Addition;
                done = true;
            }
            public override void VisitExpression(InvokeGenericExpression expression)
            {
                if (done) return;
                var old_args = expression.Arguments;
                expression.Arguments = new BaseExpression[old_args.Length + 1];
                old_args.CopyTo(expression.Arguments, 1);
                expression.Arguments[0] = Addition;
                done = true;
            }
        }

        private static BaseExpression InjectFirst(BaseExpression left, BaseExpression right)
        {
            var iv = new InjectorVisitor(left);
            iv.VisitExpression(right);
            if (!iv.done) throw new SyntaxError("Right of |> operator must be invokation", right.SourceContext);
            return right;
        }

        #region Tree Searching
        private sealed class VFirst<T>: AbstractVisitor where T: class, ExecutionContext.IExecutable
        {
            public T Found = null;
            public Func<T, bool> Predicate = x => true;
            public bool StopAtDeferred = true;

            public override void VisitExpression(FunctionDefinition expression)
            {
                if (StopAtDeferred) return;
                base.VisitExpression(expression);
            }

            public override void VisitExpression(CastExpression expression)
            {
                if (expression.as_is && StopAtDeferred) return;
                base.VisitExpression(expression);
            }

            public override void VisitExpression(BaseExpression expression)
            {
                if (expression is T t && Predicate(t))
                {
                    Found = t;
                    return;
                }
                base.VisitExpression(expression);
            }
            public override void VisitStatement(BaseStatement statement)
            {
                if (statement is T t && Predicate(t))
                {
                    Found = t;
                    return;
                }
                base.VisitStatement(statement);
            }
        }
        private static bool Any<T>(object o) where T : class, ExecutionContext.IExecutable
        {
            var has = new VFirst<T>();
            has.VisitAny(o);
            return has.Found != null;
        }
        private static bool Any<T>(object o, out T ret) where T : class, ExecutionContext.IExecutable
        {
            var has = new VFirst<T>();
            has.VisitAny(o);
            ret = has.Found;
            return ret != null;
        }
        private static bool Any<T>(object o, Func<T, bool> predicate) where T : class, ExecutionContext.IExecutable
        {
            var has = new VFirst<T>();
            has.Predicate = predicate;
            has.VisitAny(o);
            return has.Found != null;
        }
        private static bool Any<T>(object o, Func<T, bool> predicate, out T ret) where T : class, ExecutionContext.IExecutable
        {
            var has = new VFirst<T>();
            has.Predicate = predicate;
            has.VisitAny(o);
            ret = has.Found;
            return has.Found != null;
        }
        #endregion

        public static void UncachebleCheck(object o, string suffix = " after static")
        {
            if (Any<BinaryAssign>(o, out var wrongassign))
                throw new LogicalError($"Unexpected assign {wrongassign}{suffix}", wrongassign.SourceContext);
            if (Any<FunctionDefinition>(o, func => func.FunctionName != null, out var wrongmethod))
                throw new LogicalError($"Unexpected named method{suffix}", wrongmethod.SourceContext);
            if (Any<CreatorContext>(o, context => context.Name != null, out var wrongcontext))
                throw new LogicalError($"Unexpected named context{suffix}", wrongcontext.SourceContext);
            if (Any<CreatorDictionary>(o, dictioanry => dictioanry.Name != null, out var wrongdictionary))
                throw new LogicalError($"Unexpected named dictionary{suffix}", wrongdictionary.SourceContext);
            if (Any<CreatorInstance>(o, instance => instance.Name != null, out var wronginstance))
                throw new LogicalError($"Unexpected named instantation{suffix}", wronginstance.SourceContext);
            if (Any<UsingExpression>(o, @using => @using.Alias != null, out var wrongusing))
                throw new LogicalError($"Unexpected named using{suffix}", wrongusing.SourceContext);
            if (Any<ConstraintExpression>(o, constraint => constraint.Name != null, out var wrongconstraint))
                throw new LogicalError($"Unexpected named constraint{suffix}", wrongconstraint.SourceContext);
        }

        private BaseExpression ReorderStatic(StaticExpression expression)
        {
            //if (did.TryGetValue(expression, out var expr)) return expr;
            if (expression.Right is BinaryAssign)
            {
                var input = expression.CloneCast();
                var left = input.Right as BinaryAssign;
                while (left is BinaryAssign assign && assign.Right is BinaryAssign)
                    left = assign.Right as BinaryAssign;
                UncachebleCheck(left.Right);
                var ret = input.Right;
                input.Right = left.Right;
                left.Right = input;
                return ret;
            }
            if (expression.Right is CreatorContext context && context.HasName)
                return new BinaryAssign(context.Name.CloneCast(), expression, context.SourceContext);
            if (expression.Right is CreatorInstance instance && instance.Name != null)
                return new BinaryAssign(instance.Name.CloneCast(), expression, instance.SourceContext);
            if (expression.Right is UsingExpression @using && @using.Alias != null)
                return new BinaryAssign(@using.Alias.CloneCast(), expression, @using.SourceContext);
            if (expression.Right is CreatorDictionary dictionary && dictionary.Name != null)
                return new BinaryAssign(dictionary.Name.CloneCast(), expression, dictionary.SourceContext);
            if (expression.Right is ConstraintExpression constraint && constraint.Name != null)
                return new BinaryAssign(constraint.Name.CloneCast(), expression, constraint.SourceContext);
            return ReorderStaticMethod(expression);
        }
        private BaseExpression ReorderStaticMethod(StaticExpression expression)
        {
            if (expression.Right is FunctionDefinition func && func.FunctionName != null)
                return new BinaryAssign(func.FunctionName.CloneCast(), expression, expression.Right.SourceContext);
            UncachebleCheck(expression.Right);
            return expression;
        }

        public static string NumericLiteralReplacing(string numeric)
        {
            numeric = numeric.Replace("_", "");

            if (numeric.Contains("."))
            {
                var k = numeric.Count(x => x == 'k');

                if (k > 0)
                {
                    numeric = numeric.Replace("k", "");

                    var dot = numeric.IndexOf(".");
                    numeric = numeric.Replace(".", "");

                    if (dot + k*3 >= numeric.Length)
                    {
                        numeric = numeric.PadRight(dot + k * 3, '0');
                    }
                    else
                    {
                        numeric = numeric.Substring(0, dot + k * 3) + "." + numeric.Substring(dot + k * 3, numeric.Length - dot - k * 3);
                    }
                }
            }

            numeric = numeric.Replace("k", "000");

            return numeric;
        }

        private static BaseExpression Constantination(BaseExpression expression)
        {
            var target = default(NameExpression);

            if (expression is MemberAccess ma && ma.Right is NameExpression name)
                target = name;

            if (expression is NameExpression name2)
                target = name2;

            if (target != null)
                target.Const = true;

            return expression;
        }
    }
}
