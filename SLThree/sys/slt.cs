using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class slt
    {
        public static readonly List<Assembly> RegistredAssemblies = new List<Assembly>()
        {
            typeof(object).Assembly,
            typeof(slt).Assembly
        };
        public static BaseStatement parse(string s) => Parser.This.ParseScript(s);
        public static BaseExpression parse_expr(string s) => Parser.This.ParseExpression(s);
        public static object eval(string s) => Parser.This.EvalExpression(s);
        public static object eval(ExecutionContext.ContextWrap context, string s) => Parser.This.EvalExpression(s, context.pred);

        public static string repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ExecutionContext.ContextWrap wrap) => wrap.ToDetailedString(1, new List<ExecutionContext.ContextWrap>());
    }
#pragma warning restore IDE1006 // Стили именования
}
