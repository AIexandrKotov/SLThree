using Pegasus.Common;
using System;

namespace SLThree
{
    [Serializable]
    public class SLTException : Exception
    {
        public SourceContext Context { get; set; }

        public static string AtContext(SourceContext context) => context == null ? "" : $" {Locale.Current["ERR_At"]} {context}";

        public SLTException() { }
        public SLTException(SourceContext context) { Context = context; }
        public SLTException(string message, SourceContext context) : base($"{message}{AtContext(context)}") { Context = context; }
        public SLTException(string message, Exception inner, SourceContext context) : base($"{message}{AtContext(context)}", inner) { Context = context; }
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
        public SyntaxError(Cursor cursor) : base(new SourceContext(cursor)) { }
        public SyntaxError(string message, Cursor cursor) : base(message, new SourceContext(cursor)) { }
        public SyntaxError(string message, Exception inner, Cursor cursor) : base(message, inner, new SourceContext(cursor)) { }
    }

    /// <summary>
    /// Исключения на этапе создания дерева
    /// </summary>
    [Serializable]
    public class LogicalError : SLTException
    {
        public LogicalError() : base() { }
        public LogicalError(SourceContext context) : base(context) { }
        public LogicalError(string message, SourceContext context) : base(message, context) { }
        public LogicalError(string message, Exception inner, SourceContext context) : base(message, inner, context) { }
    }

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
