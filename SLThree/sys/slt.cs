using SLThree.Extensions;
using SLThree.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SLThree.sys
{
#pragma warning disable IDE1006 // Стили именования
    public static class slt
    {
        public static BaseStatement parse(string s) => DotnetEnvironment.DefaultParser.ParseScript(s, null);
        public static BaseExpression parse_expr(string s) => DotnetEnvironment.DefaultParser.ParseExpression(s, null);
        public static object eval(string s) => DotnetEnvironment.DefaultParser.EvalExpression(s);
        public static object eval(ContextWrap context, string s) => DotnetEnvironment.DefaultParser.EvalExpression(s, context.Context);

        public static object clone(ICloneable clone) => clone.Clone();

        public static string dev_repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ContextWrap wrap) => wrap.ToDetailedString(1, new List<ContextWrap>());
        public static string repr(object o, long max_depth) => XmlViewer.GetView(o, max_depth);
        public static string repr(object o) => XmlViewer.GetView(o, 1);

        public static Method make_generic(GenericMethod method, IList<object> generic_args) => method.MakeGenericMethod(generic_args.Select(x => (Type)x).ToArray());
        public static Method make_generic<T1>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1) });
        public static Method make_generic<T1, T2>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1), typeof(T2) });
        public static Method make_generic<T1, T2, T3>(GenericMethod method) => method.MakeGenericMethod(new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        public static Method make_template(TemplateMethod method, object[] args)
            => method.MakeGenericMethod(args.ConvertAll(x => (TemplateMethod.GenericMaking.Constraint, x)));
    }
#pragma warning restore IDE1006 // Стили именования
}
