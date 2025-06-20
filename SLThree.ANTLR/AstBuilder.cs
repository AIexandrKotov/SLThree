using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static SLThreeParser;

namespace SLThree.ANTLR
{
    public class AstBuilder : SLThreeBaseVisitor<BaseExpression>
    {
        private string fileName;

        public void SetSource(string newFileName)
        {
            fileName = newFileName;
        }

        private SourceContext CreateContext(ParserRuleContext context)
        {
            return new SourceContext()
            {
                Line = context.Start.Line,
                Column = context.Start.Column,
                Filename = fileName,
            };
        }

        public override BaseExpression VisitParse(ParseContext context)
        {
            return Visit(context.expression());
        }

        public override BaseExpression VisitExpression(ExpressionContext context)
        {
            var terms = context.term();
            var result = Visit(terms[0]);
            for (var i = 1; i < terms.Length; i++)
            {
                var op = context.children[i * 2 - 1] as ITerminalNode;
                var right = Visit(terms[i]);
                if (op.Symbol.Type == PLUS) result = new BinaryAdd(result, right, CreateContext(context));
            }
            return result;
        }

        public override BaseExpression VisitTerm(TermContext context)
        {
            var factors = context.factor();
            var result = Visit(factors[0]);
            for (var i = 1; i < factors.Length; i++)
            {
                var op = context.children[i * 2 - 1] as ITerminalNode;
                var right = Visit(factors[i]);
                if (op.Symbol.Type == MUL) result = new BinaryMultiply(result, right, CreateContext(context));
            }
            return result;
        }

        public override BaseExpression VisitFactor(FactorContext context)
        {
            if (context.NUMBER() is ITerminalNode number)
            {
                return new IntLiteral(int.Parse(number.GetText()), number.GetText(), CreateContext(context));
            }
            if (context.expression() is ExpressionContext expr)
            {
                return Visit(expr);
            }
            throw new System.NotImplementedException("Unknown factor");
        }
    }
} 