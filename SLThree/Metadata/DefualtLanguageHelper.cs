using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.Collections.Generic;

namespace SLThree.Metadata
{
#pragma warning disable IDE1006 // Стили именования
    public abstract class DefualtLanguageHelper<T> where T : ILanguageProvider, new()
    {
        private static T LanguageInfo = new T();

        public static BaseStatement parse(string s) => LanguageInfo.Parser.ParseScript(s, null);
        public static BaseExpression parse_expr(string s) => LanguageInfo.Parser.ParseExpression(s, null);
        public static object eval(string s) => LanguageInfo.Parser.EvalExpression(s);
        public static object eval(ContextWrap context, string s) => LanguageInfo.Parser.EvalExpression(s, context.Context);
        public static string restore(ExecutionContext.IExecutable executable, ContextWrap context)
        {
            if (executable is BaseExpression expr)
                return LanguageInfo.Restorator.Restore(expr, context?.Context);
            if (executable is BaseStatement code)
                return LanguageInfo.Restorator.Restore(code, context?.Context);
            throw new ArgumentException(nameof(executable));
        }

        public static object clone(ICloneable clone) => clone.Clone();

        public static string dev_repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ContextWrap wrap) => wrap.ToDetailedString(1, new List<ContextWrap>());
        public static string repr(object o, long max_depth) => XmlViewer.GetView(o, max_depth);
        public static string repr(object o) => XmlViewer.GetView(o, 1);

        protected DefualtLanguageHelper() { }
    }
#pragma warning restore IDE1006 // Стили именования
}