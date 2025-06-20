using Antlr4.Runtime;
using SLThree.Metadata;
using System;

namespace SLThree.ANTLR
{
    public class Parser : IParser
    {
        public BaseExpression ParseExpression(string code, string fileName)
        {
            var inputStream = new AntlrInputStream(code);
            var lexer = new SLThreeLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new SLThreeParser(commonTokenStream);

            var context = parser.parse();
            var astBuilder = new AstBuilder();
            var ret = astBuilder.Visit(context);
            return ret;

            throw new NotImplementedException();
        }

        public BaseStatement ParseScript(string code, string fileName)
        {
            // Пока не реализуем, но для чистоты можно обернуть expression в statement
            var expression = ParseExpression(code, fileName);
            //return new Expressions.ExpressionStatement(expression, null);

            throw new NotImplementedException();
        }
    }
}
