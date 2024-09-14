using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SLThree.Native
{
    public sealed class Support : AbstractVisitor
    {
        private List<Exception> Exceptions = new List<Exception>();

        static Type[] SupportedExpressions, SupportedStatements;
        private Support() { }
        static Support()
        {
            SupportedExpressions = new Type[]
            {
                typeof(LongLiteral),
                typeof(NameExpression),

                typeof(BinaryAdd),
                typeof(BinaryRem),
                typeof(BinaryMultiply),
                typeof(BinaryDivide),
            };
            SupportedStatements = new Type[]
            {
                typeof(ExpressionStatement),
                typeof(ReturnStatement),
            };
        }

        public override void VisitExpression(BaseExpression expression)
        {
            var type = expression.GetType();
            if (!SupportedExpressions.Contains(type))
                Exceptions.Add(new SupportException($"{type}", expression.SourceContext));
            base.VisitExpression(expression);
        }
        public override void VisitStatement(BaseStatement statement)
        {
            var type = statement.GetType();
            if (!SupportedStatements.Contains(type))
                Exceptions.Add(new SupportException($"{type}", statement.SourceContext));
            base.VisitStatement(statement);
        }

        public static void CheckOnSupporting(Method method)
        {
            var sup = new Support();
            sup.Visit(method);
            if (sup.Exceptions.Count > 0)
                throw new NotSupportedException($"Method {method.Name} contains unsupported elements\n{sup.Exceptions.JoinIntoString("\n")}");
        }
    }


    [Serializable]
    public class SupportException : LogicalError
    {
        public SupportException() { }

        public SupportException(ISourceContext context) : base(context) { }

        public SupportException(string message, ISourceContext context) : base(message, context) { }

        public SupportException(string message, Exception inner, ISourceContext context) : base(message, inner, context) { }
    }
}
