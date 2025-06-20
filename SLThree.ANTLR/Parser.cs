using Antlr4.Runtime;
using SLThree.Metadata;

namespace SLThree.ANTLR
{
    public class Parser : IParser
    {
        private readonly AstBuilder astBuilder = new AstBuilder();
        private readonly SLThreeLexer lexer = new SLThreeLexer(new AntlrInputStream());
        private readonly SLThreeParser parser;
        private readonly CommonTokenStream tokenStream;

        public Parser()
        {
            tokenStream = new CommonTokenStream(lexer);
            parser = new SLThreeParser(tokenStream);
            parser.RemoveParseListeners();
            parser.BuildParseTree = true;
        }

        public BaseExpression ParseExpression(string code, string fileName)
        {
            var inputStream = new AntlrInputStream(code);
            lexer.SetInputStream(inputStream);
            tokenStream.SetTokenSource(lexer);
            parser.TokenStream = tokenStream;

            var context = parser.parse();
            return astBuilder.Visit(context);
        }

        public BaseStatement ParseScript(string code, string fileName)
        {
            var expression = ParseExpression(code, fileName);
            return new ExpressionStatement(expression, null);
        }
    }
}
