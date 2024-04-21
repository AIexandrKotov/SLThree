using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class slt
    {
        public static BaseStatement parse(string s) => Parser.This.ParseScript(s);
        public static BaseExpression parse_expr(string s) => Parser.This.ParseExpression(s);
        public static object eval(string s) => Parser.This.EvalExpression(s);
        public static object eval(ContextWrap context, string s) => Parser.This.EvalExpression(s, context.Context);

        public static string repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ContextWrap wrap) => wrap.ToDetailedString(1, new List<ContextWrap>());
        public static string xml_repr(object o) => XmlViewer.GetView(o);

        public static Method make_generic(GenericMethod method, IList<object> generic_args) => method.MakeGenericMethod(generic_args.Select(x => (Type)x).ToArray());
        public static Method make_generic<T1>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1) });
        public static Method make_generic<T1, T2>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1), typeof(T2) });
        public static Method make_generic<T1, T2, T3>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1), typeof(T2), typeof(T3) });

        public static readonly List<Assembly> registred;
        static slt()
        {
            registred = AppDomain.CurrentDomain.GetAssemblies().ToList();
#if NET || NETSTANDARD
            registred.Add(typeof(Console).Assembly);
#endif
        }
        public static readonly Dictionary<string, Type> sys_types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<") && x.IsPublic).ToDictionary(x => x.Name, x => x);
    }
#pragma warning restore IDE1006 // Стили именования
}
