﻿using SLThree.Visitors;
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
        public static BaseStatement parse(string s) => Parser.This.ParseScript(s);
        public static BaseExpression parse_expr(string s) => Parser.This.ParseExpression(s);
        public static object eval(string s) => Parser.This.EvalExpression(s);
        public static object eval(ExecutionContext.ContextWrap context, string s) => Parser.This.EvalExpression(s, context.pred);

        public static string repr(object o) => TreeViewer.GetView(o);
        public static string context_repr(ExecutionContext.ContextWrap wrap) => wrap.ToDetailedString(1, new List<ExecutionContext.ContextWrap>());

        public static readonly List<Assembly> registred = new List<Assembly>()
        {
            typeof(object).Assembly,
            typeof(slt).Assembly
        };
        public static readonly Dictionary<string, Type> sys_types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.FullName.StartsWith("SLThree.sys.") && !x.Name.StartsWith("<")).ToDictionary(x => x.Name, x => x);
    }
#pragma warning restore IDE1006 // Стили именования
}
