using SLThree;
using SLThree.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace slt
{
    public static class LANG_060
    {
        public static bool Supports { get; internal set; }
        private static FieldInfo slt_RegistredAssemblies;

        public static object Eval(ExecutionContext.ContextWrap context, string str)
            => slt_eval.Invoke(null, new object[] { context, str });
        private static MethodInfo slt_eval;
        private static Type slt;

        internal static void Init()
        {
            slt = Program.SLThreeAssembly.GetType("SLThree.sys.slt");
            slt_RegistredAssemblies = slt.GetField("registred");
            slt_RegistredAssemblies.GetValue(null).Cast<List<Assembly>>().Add(typeof(LANG_060).Assembly);
            slt_eval = slt.GetMethod("eval", new Type[] { typeof(ExecutionContext.ContextWrap), typeof(string) });
        }
    }
}
