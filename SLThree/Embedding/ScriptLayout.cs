using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SLThree.Embedding
{
    public class ScriptLayout
    {
        private class LoadOption
        {
            public string file = null;
            public string dir;
            public bool all = false;
            public LoadOption() { }
        }

        private static ExecutionContext.ContextWrap Build(LoadOption loadOption)
        {
            var context = new ExecutionContext();
            if (loadOption.file != null)
            {
                Parser.This.ParseScript(File.ReadAllText(loadOption.file, Encoding.UTF8), loadOption.file).GetValue(context);
            }
            else
            {
                foreach (var x in Directory.GetFiles(loadOption.dir, "*.slt", loadOption.all ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    Parser.This.ParseScript(File.ReadAllText(x, Encoding.UTF8), x).GetValue(context);
            }
            return context.wrap;
        }

        private static bool IsScriptLayout(ExecutionContext.ContextWrap context, out LoadOption loadOption)
        {
            foreach (var x in context.pred.LocalVariables.GetAsDictionary())
            {
                if (x.Value is LoadOption lp)
                {
                    loadOption = lp;
                    return true;
                }
            }
            loadOption = null;
            return false;
        }

        private static void Reference(string assembly)
        {
            var ass = Assembly.LoadFrom(assembly);
            if (!TypeofExpression.RegistredAssemblies.Contains(ass))
                TypeofExpression.RegistredAssemblies.Add(ass);
        }

        private static BaseStatement ParseFile(string filename)
        {
            return Parser.This.ParseScript(File.ReadAllText(filename), filename);
        }

        private static BaseStatement ParseCode(string code)
        {
            return Parser.This.ParseScript(code);
        }

        private static ExecutionContext Prepare(BaseStatement st)
        {
            var context = new ExecutionContext();

            context.LocalVariables.SetValue("REFERENCE", ((Action<string>)Reference).Method);
            context.LocalVariables.SetValue("LOADOPT", new MemberAccess.ClassAccess(typeof(LoadOption)));

            st.GetValue(context);

            return context;
        }

        public static void ExecuteFile(string filename, ExecutionContext targetContext = null)
        {
            if (targetContext == null) targetContext = ExecutionContext.global.pred;

            var context = Prepare(ParseFile(filename));

            foreach (var layout in context.LocalVariables.GetAsDictionary())
            {
                if (layout.Value is ExecutionContext.ContextWrap wrap)
                {
                    if (IsScriptLayout(wrap, out var lp))
                    {
                        targetContext.LocalVariables.SetValue(layout.Key, Build(lp));
                    }
                }
            }
        }
        public static void Execute(string code, ExecutionContext targetContext = null)
        {
            if (targetContext == null) targetContext = ExecutionContext.global.pred;

            var context = Prepare(ParseCode(code));

            foreach (var layout in context.LocalVariables.GetAsDictionary())
            {
                if (layout.Value is ExecutionContext.ContextWrap wrap)
                {
                    if (IsScriptLayout(wrap, out var lp))
                    {
                        targetContext.LocalVariables.SetValue(layout.Key, Build(lp));
                    }
                }
            }
        }
    }
}
