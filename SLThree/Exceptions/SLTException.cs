
using System;

namespace SLThree
{
    [Serializable]
    public class SLTException : Exception
    {
        public ISourceContext Context { get; set; }

        public static string AtContext(ISourceContext context) => context == null ? "" : $" {Locale.Current["ERR_At"]} {context}";

        public SLTException() { }
        public SLTException(ISourceContext context) { Context = context; }
        public SLTException(string message, ISourceContext context) : base($"{message}{AtContext(context)}") { Context = context; }
        public SLTException(string message, Exception inner, ISourceContext context) : base($"{message}{AtContext(context)}", inner) { Context = context; }
    }

    /// <summary>
    /// Исключения на этапе парсинга
    /// </summary>
    [Serializable]
    public class LexicalError : SLTException
    {
        public LexicalError() : base() { }
        public LexicalError(ISourceContext cursor) : base(cursor) { }
        public LexicalError(string message, ISourceContext cursor) : base(message, cursor) { }
        public LexicalError(string message, Exception inner, ISourceContext cursor) : base(message, inner, cursor) { }
    }

    /// <summary>
    /// Исключения на этапе обработки синтаксиса
    /// </summary>
    [Serializable]
    public class SyntaxError : SLTException
    {
        public SyntaxError() : base() { }
        public SyntaxError(ISourceContext context) : base(context) { }
        public SyntaxError(string message, ISourceContext context) : base(message, context) { }
        public SyntaxError(string message, Exception inner, ISourceContext context) : base(message, inner, context) { }
    }

    /// <summary>
    /// Исключения на этапе создания дерева
    /// </summary>
    [Serializable]
    public class LogicalError : SLTException
    {
        public LogicalError() : base() { }
        public LogicalError(ISourceContext context) : base(context) { }
        public LogicalError(string message, ISourceContext context) : base(message, context) { }
        public LogicalError(string message, Exception inner, ISourceContext context) : base(message, inner, context) { }
    }

    /// <summary>
    /// Исключения во время работы программы
    /// </summary>
    [Serializable]
    public class RuntimeError : SLTException
    {
        public RuntimeError() : base() { }
        public RuntimeError(ISourceContext context) : base(context) { }
        public RuntimeError(string message, ISourceContext context) : base(message, context) { }
        public RuntimeError(string message, Exception inner, ISourceContext context) : base(message, inner, context) { }
    }
}
