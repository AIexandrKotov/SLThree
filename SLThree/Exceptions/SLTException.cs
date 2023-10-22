using Pegasus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SLThree
{
    [Serializable]
    public class SLTException : Exception
    {
        public SourceContext Context { get; set; }

        public SLTException() { }
        public SLTException(SourceContext context) { Context = context; }
        public SLTException(string message, SourceContext context) : base($"{message} at {context}") { Context = context; }
        public SLTException(string message, Exception inner, SourceContext context) : base($"{message} at {context}", inner) { Context = context; }
    }

    /// <summary>
    /// Исключения на этапе парсинга
    /// </summary>
    [Serializable]
    public class LexicalError : SLTException
    {
        public LexicalError() : base() { }
        public LexicalError(Cursor cursor) : base(new SourceContext(cursor)) { }
        public LexicalError(string message, Cursor cursor) : base(message, new SourceContext(cursor)) { }
        public LexicalError(string message, Exception inner, Cursor cursor) : base(message, inner, new SourceContext(cursor)) { }
    }

    /// <summary>
    /// Исключения на этапе обработки синтаксиса
    /// </summary>
    [Serializable]
    public class SyntaxError : SLTException
    {
        public SyntaxError() : base() { }
        public SyntaxError(SourceContext context) : base(context) { }
        public SyntaxError(string message, SourceContext context) : base(message, context) { }
        public SyntaxError(string message, Exception inner, SourceContext context) : base(message, inner, context) { }
    }

/*    /// <summary>
    /// Исключения на этапе переработки AST в Tree
    /// </summary>
    [Serializable]
    public class SemanticError : SLTException
    {
        public SemanticError() : base() { }
        public SemanticError(SourceContext context) : base(context) { }
        public SemanticError(string message, SourceContext context) : base(message, context) { }
        public SemanticError(string message, Exception inner, SourceContext context) : base(message, inner, context) { }
    }*/

    /// <summary>
    /// Исключения во время работы программы
    /// </summary>
    [Serializable]
    public class RuntimeError : SLTException
    {
        public RuntimeError() : base() { }
        public RuntimeError(SourceContext context) : base(context) { }
        public RuntimeError(string message, SourceContext context) : base(message, context) { }
        public RuntimeError(string message, Exception inner, SourceContext context) : base(message, inner, context) { }
    }
}
